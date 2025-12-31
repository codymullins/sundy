namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Result of a successful authentication flow.
/// </summary>
public record AuthResult(
    string AccountId,
    string Email,
    string DisplayName,
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset? ExpiresAt
);

/// <summary>
/// Provides OAuth authentication for Microsoft Graph API.
/// Implemented by platform-specific projects (Blazor, Desktop, etc.).
/// </summary>
public interface IMicrosoftAuthProvider
{
    /// <summary>
    /// Initiates an interactive authentication flow to add a new account.
    /// Returns authentication result on success.
    /// </summary>
    Task<AuthResult> AuthenticateAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets an access token for a previously authenticated account.
    /// Handles token refresh automatically if needed.
    /// Returns null if the account is not authenticated or tokens cannot be refreshed.
    /// </summary>
    Task<string?> GetAccessTokenAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Signs out and removes tokens for an account.
    /// </summary>
    Task SignOutAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Checks if an account has valid (non-expired) authentication.
    /// </summary>
    bool IsAuthenticated(string accountId);

    /// <summary>
    /// Event fired when device code authentication is needed.
    /// UI should display the verification URL and user code.
    /// </summary>
    event EventHandler<DeviceCodeInfo>? DeviceCodeReceived;
}
