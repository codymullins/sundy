using System.Collections.Concurrent;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Manages multiple Microsoft/Outlook account connections.
/// Platform-agnostic - delegates authentication to IMicrosoftAuthProvider.
/// </summary>
public class MicrosoftAccountManager : IMicrosoftAccountManager
{
    private readonly ILogger<MicrosoftAccountManager> _log;
    private readonly IConnectedAccountStore _accountStore;
    private readonly IMicrosoftAuthProvider _authProvider;
    private readonly ConcurrentDictionary<string, GraphServiceClient> _clientsByAccountId = new();
    private readonly ConcurrentDictionary<string, ConnectedAccount> _accounts = new();

    private static readonly string[] Scopes = ["user.read", "Calendars.ReadWrite"];

    public event EventHandler<DeviceCodeInfo>? DeviceCodeReceived;

    public MicrosoftAccountManager(
        ILogger<MicrosoftAccountManager> log,
        IConnectedAccountStore accountStore,
        IMicrosoftAuthProvider authProvider)
    {
        _log = log;
        _accountStore = accountStore;
        _authProvider = authProvider;

        // Forward device code events from auth provider
        _authProvider.DeviceCodeReceived += (s, e) => DeviceCodeReceived?.Invoke(this, e);
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        // Load accounts from the store
        var storedAccounts = await _accountStore.GetByProviderTypeAsync(ProviderType.Microsoft, ct);
        foreach (var account in storedAccounts)
        {
            _accounts[account.Id] = account;
        }

        _log.LogInformation("Loaded {Count} Microsoft accounts from store", storedAccounts.Count);

        // Validate authentication status for each account
        await ValidateAccountsAsync(ct);
    }

    private async Task ValidateAccountsAsync(CancellationToken ct)
    {
        foreach (var account in _accounts.Values)
        {
            if (!_authProvider.IsAuthenticated(account.Id))
            {
                account.Status = AccountStatus.Expired;
                await _accountStore.UpdateAsync(account, ct);
                _log.LogWarning("Account {Email} marked as expired - authentication invalid", account.Email);
            }
        }
    }

    public IReadOnlyList<ConnectedAccount> GetConnectedAccounts()
    {
        return _accounts.Values.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<ConnectedAccount>> GetConnectedAccountsAsync(CancellationToken ct = default)
    {
        if (_accounts.IsEmpty)
        {
            await InitializeAsync(ct);
        }
        return GetConnectedAccounts();
    }

    public async Task<ConnectedAccount> AddAccountAsync(CancellationToken ct = default)
    {
        _log.LogInformation("Starting authentication for new Microsoft account");

        var result = await _authProvider.AuthenticateAsync(ct);

        // Check if this account is already connected
        if (_accounts.TryGetValue(result.AccountId, out var existingAccount))
        {
            _log.LogInformation("Account {Email} was already connected, refreshing", result.Email);
            existingAccount.Status = AccountStatus.Connected;
            existingAccount.LastSyncAt = DateTimeOffset.UtcNow;
            existingAccount.AccessToken = result.AccessToken;
            existingAccount.RefreshToken = result.RefreshToken;
            existingAccount.TokenExpiresAt = result.ExpiresAt;
            await _accountStore.UpdateAsync(existingAccount, ct);

            // Clear cached client to force re-creation with new token
            _clientsByAccountId.TryRemove(result.AccountId, out _);

            return existingAccount;
        }

        // Create new connected account
        var connectedAccount = new ConnectedAccount
        {
            Id = result.AccountId,
            Email = result.Email,
            DisplayName = result.DisplayName,
            ProviderType = ProviderType.Microsoft,
            ConnectedAt = DateTimeOffset.UtcNow,
            Status = AccountStatus.Connected,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            TokenExpiresAt = result.ExpiresAt
        };

        // Save to store and cache
        await _accountStore.AddAsync(connectedAccount, ct);
        _accounts[result.AccountId] = connectedAccount;

        _log.LogInformation("Successfully connected Microsoft account: {Email}", connectedAccount.Email);

        return connectedAccount;
    }

    public async Task RemoveAccountAsync(string accountId, CancellationToken ct = default)
    {
        if (!_accounts.TryRemove(accountId, out var account))
        {
            _log.LogWarning("Account {AccountId} not found in cache", accountId);
            return;
        }

        // Sign out from auth provider
        await _authProvider.SignOutAsync(accountId, ct);

        // Remove cached client
        _clientsByAccountId.TryRemove(accountId, out _);

        // Remove from store
        await _accountStore.DeleteAsync(accountId, ct);

        _log.LogInformation("Removed Microsoft account: {Email}", account.Email);
    }

    public async Task<GraphServiceClient?> GetClientForAccountAsync(string accountId, CancellationToken ct = default)
    {
        // Return cached client if available and token is still valid
        if (_clientsByAccountId.TryGetValue(accountId, out var cachedClient))
        {
            // Check if we need to refresh the token
            if (_accounts.TryGetValue(accountId, out var account) &&
                account.TokenExpiresAt.HasValue &&
                account.TokenExpiresAt.Value > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                return cachedClient;
            }

            // Token expiring soon, remove cached client
            _clientsByAccountId.TryRemove(accountId, out _);
        }

        // Get fresh access token from auth provider
        var accessToken = await _authProvider.GetAccessTokenAsync(accountId, ct);

        if (string.IsNullOrEmpty(accessToken))
        {
            _log.LogWarning("Failed to get access token for account {AccountId}", accountId);

            // Mark account as expired
            if (_accounts.TryGetValue(accountId, out var account))
            {
                account.Status = AccountStatus.Expired;
                await _accountStore.UpdateAsync(account, ct);
            }

            return null;
        }

        // Create new client with fresh token
        var tokenCredential = new StaticTokenCredential(accessToken);
        var client = new GraphServiceClient(tokenCredential, Scopes);
        _clientsByAccountId[accountId] = client;

        return client;
    }

    public bool IsAccountAuthenticated(string accountId)
    {
        return _accounts.TryGetValue(accountId, out var account) &&
               account.Status == AccountStatus.Connected &&
               _authProvider.IsAuthenticated(accountId);
    }

    public async Task<bool> RefreshAccountAsync(string accountId, CancellationToken ct = default)
    {
        if (!_accounts.TryGetValue(accountId, out var account))
        {
            return false;
        }

        try
        {
            // Remove cached client to force re-creation
            _clientsByAccountId.TryRemove(accountId, out _);

            // Try to get a fresh token
            var accessToken = await _authProvider.GetAccessTokenAsync(accountId, ct);
            if (!string.IsNullOrEmpty(accessToken))
            {
                account.Status = AccountStatus.Connected;
                account.LastSyncAt = DateTimeOffset.UtcNow;
                await _accountStore.UpdateAsync(account, ct);
                return true;
            }

            // Token refresh failed, mark as expired
            account.Status = AccountStatus.Expired;
            await _accountStore.UpdateAsync(account, ct);
            return false;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to refresh account {Email}", account.Email);
            account.Status = AccountStatus.Error;
            await _accountStore.UpdateAsync(account, ct);
            return false;
        }
    }
}

/// <summary>
/// Simple TokenCredential that returns a static access token.
/// Used to create GraphServiceClient with a pre-obtained token.
/// </summary>
internal class StaticTokenCredential : TokenCredential
{
    private readonly string _accessToken;

    public StaticTokenCredential(string accessToken)
    {
        _accessToken = accessToken;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1));
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new AccessToken(_accessToken, DateTimeOffset.UtcNow.AddHours(1)));
    }
}
