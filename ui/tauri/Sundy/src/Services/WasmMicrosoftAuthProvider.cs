using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using Sundy.Core.Calendars.Outlook;

namespace Sundy.Services;

/// <summary>
/// WASM-compatible Microsoft auth provider using Authorization Code Flow with PKCE.
/// Uses popup-based authentication and Tauri commands for token exchange (to bypass CORS).
/// </summary>
public class WasmMicrosoftAuthProvider : IMicrosoftAuthProvider
{
    private readonly IConnectedAccountStore _accountStore;
    private readonly OutlookGraphOptions _options;
    private readonly IJSRuntime _jsRuntime;

    private const string AuthorizeEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
    private static readonly string[] Scopes = ["user.read", "Calendars.ReadWrite", "offline_access"];

    private bool? _isTauri;

    public event EventHandler<DeviceCodeInfo>? DeviceCodeReceived;

    public WasmMicrosoftAuthProvider(
        IConnectedAccountStore accountStore,
        IJSRuntime jsRuntime,
        OutlookGraphOptions? options = null)
    {
        _accountStore = accountStore;
        _jsRuntime = jsRuntime;
        _options = options ?? new OutlookGraphOptions();
    }

    public async Task<AuthResult> AuthenticateAsync(CancellationToken ct = default)
    {
        var isTauri = await IsTauriAsync();

        // Generate PKCE code verifier and challenge
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = Guid.NewGuid().ToString("N");

        // Build authorization URL with dynamic redirect URI
        var redirectUri = await GetRedirectUriAsync();
        var authUrl = BuildAuthorizationUrl(codeChallenge, state, redirectUri);

        // Open popup and wait for callback
        var authResult = await _jsRuntime.InvokeAsync<AuthCallbackResult>("openAuthPopup", ct, authUrl, state);

        if (string.IsNullOrEmpty(authResult?.Code))
        {
            throw new InvalidOperationException("No authorization code received");
        }

        TokenResponse? tokenResponse;

        if (isTauri)
        {
            // Tauri mode: Exchange code for tokens via Tauri command (bypasses CORS)
            tokenResponse = await _jsRuntime.InvokeAsync<TokenResponse>(
                "__TAURI__.core.invoke",
                ct,
                "exchange_oauth_code",
                new
                {
                    code = authResult.Code,
                    codeVerifier = codeVerifier,
                    redirectUri = redirectUri
                });
        }
        else
        {
            // Browser mode: Exchange code directly via JavaScript fetch
            tokenResponse = await _jsRuntime.InvokeAsync<TokenResponse>(
                "exchangeOAuthCode",
                ct,
                authResult.Code,
                codeVerifier,
                redirectUri,
                _options.ClientId);
        }

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Failed to obtain access token");
        }

        // Get user info
        var (email, displayName) = await GetUserInfoAsync(tokenResponse.AccessToken, ct);

        // Create account ID
        var accountId = $"wasm_{email}";

        return new AuthResult(
            accountId,
            email,
            displayName,
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
        );
    }

    public async Task<string?> GetAccessTokenAsync(string accountId, CancellationToken ct = default)
    {
        // Get account from store to check token status
        var accounts = await _accountStore.GetByProviderTypeAsync(ProviderType.Microsoft, ct);
        var account = accounts.FirstOrDefault(a => a.Id == accountId);

        if (account == null)
        {
            return null;
        }

        // Check if token is still valid (with 5 minute buffer)
        if (account.TokenExpiresAt.HasValue &&
            account.TokenExpiresAt.Value > DateTimeOffset.UtcNow.AddMinutes(5) &&
            !string.IsNullOrEmpty(account.AccessToken))
        {
            return account.AccessToken;
        }

        // Need to refresh the token
        if (string.IsNullOrEmpty(account.RefreshToken))
        {
            return null;
        }

        var isTauri = await IsTauriAsync();
        TokenResponse? tokenResponse;

        try
        {
            if (isTauri)
            {
                // Tauri mode: Refresh token via Tauri command (bypasses CORS)
                tokenResponse = await _jsRuntime.InvokeAsync<TokenResponse>(
                    "__TAURI__.core.invoke",
                    ct,
                    "refresh_oauth_token",
                    new { refreshToken = account.RefreshToken });
            }
            else
            {
                // Browser mode: Refresh token directly via JavaScript fetch
                tokenResponse = await _jsRuntime.InvokeAsync<TokenResponse>(
                    "refreshOAuthToken",
                    ct,
                    account.RefreshToken,
                    _options.ClientId);
            }

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return null;
            }

            // Update account with new tokens
            account.AccessToken = tokenResponse.AccessToken;
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                account.RefreshToken = tokenResponse.RefreshToken;
            }
            account.TokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            account.Status = AccountStatus.Connected;

            await _accountStore.UpdateAsync(account, ct);

            return tokenResponse.AccessToken;
        }
        catch
        {
            return null;
        }
    }

    public Task SignOutAsync(string accountId, CancellationToken ct = default)
    {
        // WASM doesn't have a local token cache to clear beyond the database
        // The MicrosoftAccountManager handles removing from store
        return Task.CompletedTask;
    }

    public bool IsAuthenticated(string accountId)
    {
        // For WASM, we check stored tokens synchronously isn't ideal
        // but the MicrosoftAccountManager caches accounts, so we can check there
        // This is a best-effort check - GetAccessTokenAsync does the real validation
        return true; // Defer to GetAccessTokenAsync for actual validation
    }

    private async Task<bool> IsTauriAsync()
    {
        if (_isTauri.HasValue)
        {
            return _isTauri.Value;
        }

        try
        {
            _isTauri = await _jsRuntime.InvokeAsync<bool>("isTauriEnvironment");
        }
        catch
        {
            _isTauri = false;
        }

        return _isTauri.Value;
    }

    private async Task<string> GetRedirectUriAsync()
    {
        try
        {
            var origin = await _jsRuntime.InvokeAsync<string>("eval", "window.location.origin");
            return $"{origin}/auth-callback.html";
        }
        catch
        {
            return "http://localhost:1420/auth-callback.html";
        }
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private string BuildAuthorizationUrl(string codeChallenge, string state, string redirectUri)
    {
        var scopeString = string.Join(" ", Scopes);
        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId!,
            ["response_type"] = "code",
            ["redirect_uri"] = redirectUri,
            ["scope"] = scopeString,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256",
            ["prompt"] = "select_account"
        };

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{AuthorizeEndpoint}?{queryString}";
    }

    private static async Task<(string email, string displayName)> GetUserInfoAsync(string accessToken, CancellationToken ct)
    {
        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<GraphUserResponse>(cancellationToken: ct);
        return (user?.Mail ?? user?.UserPrincipalName ?? "unknown@unknown.com", user?.DisplayName ?? "Unknown");
    }

    // Response DTOs
    private sealed class AuthCallbackResult
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }

    private sealed class GraphUserResponse
    {
        [JsonPropertyName("mail")]
        public string? Mail { get; set; }

        [JsonPropertyName("userPrincipalName")]
        public string? UserPrincipalName { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }
}
