using Microsoft.Graph;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Manages multiple Microsoft/Outlook account connections.
/// </summary>
public interface IMicrosoftAccountManager
{
    /// <summary>
    /// Event fired when device code is received during authentication.
    /// </summary>
    event EventHandler<DeviceCodeInfo>? DeviceCodeReceived;

    /// <summary>
    /// Get all connected Microsoft accounts.
    /// </summary>
    IReadOnlyList<ConnectedAccount> GetConnectedAccounts();

    /// <summary>
    /// Get all connected Microsoft accounts asynchronously (loads from store).
    /// </summary>
    Task<IReadOnlyList<ConnectedAccount>> GetConnectedAccountsAsync(CancellationToken ct = default);

    /// <summary>
    /// Add a new Microsoft account via authentication flow.
    /// Returns the connected account on success.
    /// </summary>
    Task<ConnectedAccount> AddAccountAsync(CancellationToken ct = default);

    /// <summary>
    /// Remove a connected account and sign out.
    /// </summary>
    Task RemoveAccountAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Get the GraphServiceClient for a specific account.
    /// Returns null if the account is not connected or tokens are expired.
    /// </summary>
    Task<GraphServiceClient?> GetClientForAccountAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Check if an account is currently authenticated.
    /// </summary>
    bool IsAccountAuthenticated(string accountId);

    /// <summary>
    /// Refresh/re-authenticate an account.
    /// </summary>
    Task<bool> RefreshAccountAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Initialize the manager and load accounts from the store.
    /// </summary>
    Task InitializeAsync(CancellationToken ct = default);
}
