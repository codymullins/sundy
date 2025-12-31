using System.Collections.Concurrent;
using Microsoft.Identity.Client;
using Sundy.Core.Calendars.Outlook;

namespace Sundy.Services;

/// <summary>
/// Desktop Microsoft auth provider using MSAL (Microsoft Authentication Library).
/// Supports interactive browser and device code authentication flows.
/// </summary>
public class DesktopMicrosoftAuthProvider : IMicrosoftAuthProvider
{
    private readonly IPublicClientApplication _pca;
    private readonly OutlookGraphOptions _options;
    private readonly string[] _scopes = ["user.read", "Calendars.ReadWrite"];

    // Track which account IDs are authenticated in MSAL
    private readonly ConcurrentDictionary<string, bool> _authenticatedAccounts = new();

    public event EventHandler<DeviceCodeInfo>? DeviceCodeReceived;

    public DesktopMicrosoftAuthProvider(OutlookGraphOptions? options = null)
    {
        _options = options ?? new OutlookGraphOptions();

        var builder = PublicClientApplicationBuilder
            .Create(_options.ClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _options.TenantId);

        // Only add redirect URI for interactive flow (not needed for device code)
        if (!_options.UseDeviceCodeFlow)
        {
            builder = builder.WithDefaultRedirectUri();
        }

        _pca = builder.Build();
    }

    public async Task<AuthResult> AuthenticateAsync(CancellationToken ct = default)
    {
        AuthenticationResult result;

        if (_options.UseDeviceCodeFlow)
        {
            result = await AcquireTokenWithDeviceCodeAsync(ct);
        }
        else
        {
            result = await AcquireTokenInteractiveAsync(ct);
        }

        var accountId = result.Account.HomeAccountId.Identifier;
        _authenticatedAccounts[accountId] = true;

        // Get display name via Graph API
        var displayName = await GetUserDisplayNameAsync(result.AccessToken, ct)
            ?? result.Account.Username
            ?? "Unknown";

        return new AuthResult(
            accountId,
            result.Account.Username ?? "Unknown",
            displayName,
            result.AccessToken,
            null, // MSAL manages refresh tokens internally
            result.ExpiresOn
        );
    }

    public async Task<string?> GetAccessTokenAsync(string accountId, CancellationToken ct = default)
    {
        var msalAccounts = await _pca.GetAccountsAsync();
        var msalAccount = msalAccounts.FirstOrDefault(a => a.HomeAccountId.Identifier == accountId);

        if (msalAccount == null)
        {
            _authenticatedAccounts.TryRemove(accountId, out _);
            return null;
        }

        try
        {
            var result = await _pca.AcquireTokenSilent(_scopes, msalAccount)
                .ExecuteAsync(ct);

            _authenticatedAccounts[accountId] = true;
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            // Token expired, need re-authentication
            _authenticatedAccounts.TryRemove(accountId, out _);
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task SignOutAsync(string accountId, CancellationToken ct = default)
    {
        var msalAccounts = await _pca.GetAccountsAsync();
        var msalAccount = msalAccounts.FirstOrDefault(a => a.HomeAccountId.Identifier == accountId);

        if (msalAccount != null)
        {
            await _pca.RemoveAsync(msalAccount);
        }

        _authenticatedAccounts.TryRemove(accountId, out _);
    }

    public bool IsAuthenticated(string accountId)
    {
        return _authenticatedAccounts.TryGetValue(accountId, out var value) && value;
    }

    private async Task<AuthenticationResult> AcquireTokenInteractiveAsync(CancellationToken ct)
    {
        return await _pca.AcquireTokenInteractive(_scopes)
            .WithPrompt(Prompt.SelectAccount) // Always show account picker for multi-account
            .ExecuteAsync(ct);
    }

    private async Task<AuthenticationResult> AcquireTokenWithDeviceCodeAsync(CancellationToken ct)
    {
        return await _pca.AcquireTokenWithDeviceCode(_scopes, deviceCodeResult =>
        {
            var deviceCodeInfo = new DeviceCodeInfo
            {
                VerificationUrl = deviceCodeResult.VerificationUrl?.ToString() ?? "https://microsoft.com/devicelogin",
                UserCode = deviceCodeResult.UserCode ?? "UNKNOWN",
                FullMessage = deviceCodeResult.Message,
                ExpiresOn = deviceCodeResult.ExpiresOn
            };
            DeviceCodeReceived?.Invoke(this, deviceCodeInfo);
            return Task.CompletedTask;
        }).ExecuteAsync(ct);
    }

    private static async Task<string?> GetUserDisplayNameAsync(string accessToken, CancellationToken ct)
    {
        try
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(request, ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                // Simple JSON parsing for displayName
                var displayNameStart = json.IndexOf("\"displayName\":", StringComparison.Ordinal);
                if (displayNameStart >= 0)
                {
                    var valueStart = json.IndexOf('"', displayNameStart + 14) + 1;
                    var valueEnd = json.IndexOf('"', valueStart);
                    if (valueEnd > valueStart)
                    {
                        return json[valueStart..valueEnd];
                    }
                }
            }
        }
        catch
        {
            // Ignore errors getting display name
        }
        return null;
    }
}
