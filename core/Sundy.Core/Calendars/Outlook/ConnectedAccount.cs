namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Status of a connected external account.
/// </summary>
public enum AccountStatus
{
    /// <summary>Account is connected and tokens are valid.</summary>
    Connected = 0,

    /// <summary>Account tokens have expired and need re-authentication.</summary>
    Expired = 1,

    /// <summary>Account encountered an error during sync or auth.</summary>
    Error = 2
}

/// <summary>
/// Represents a connected Microsoft/Outlook account.
/// </summary>
public class ConnectedAccount
{
    /// <summary>
    /// Unique identifier for the account (MSAL account home ID).
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// User's email address or UPN.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's display name.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Provider type for future extensibility (Microsoft, Google, etc.).
    /// </summary>
    public ProviderType ProviderType { get; set; } = ProviderType.Microsoft;

    /// <summary>
    /// When the account was first connected.
    /// </summary>
    public DateTimeOffset ConnectedAt { get; set; }

    /// <summary>
    /// When the account was last successfully synced.
    /// </summary>
    public DateTimeOffset? LastSyncAt { get; set; }

    /// <summary>
    /// Current status of the account connection.
    /// </summary>
    public AccountStatus Status { get; set; } = AccountStatus.Connected;

    /// <summary>
    /// OAuth access token (for WASM - stored directly since no MSAL cache).
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// OAuth refresh token for token renewal.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the access token expires.
    /// </summary>
    public DateTimeOffset? TokenExpiresAt { get; set; }
}

/// <summary>
/// Type of calendar provider.
/// </summary>
public enum ProviderType
{
    Microsoft = 1,
    Google = 2  // Future support
}
