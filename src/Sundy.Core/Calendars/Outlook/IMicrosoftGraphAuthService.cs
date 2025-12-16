namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Interface for Microsoft Graph authentication services.
/// Implemented differently for desktop (using Azure.Identity) and browser (using MSAL.js).
/// </summary>
public interface IMicrosoftGraphAuthService
{
    /// <summary>
    /// Gets whether the user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the display name of the authenticated user.
    /// </summary>
    string? UserDisplayName { get; }

    /// <summary>
    /// Event raised when device code info is received (desktop only).
    /// </summary>
    event EventHandler<DeviceCodeInfo>? DeviceCodeReceived;

    /// <summary>
    /// Authenticates the user using the default method for the platform.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if authentication succeeded</returns>
    Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates using device code flow (desktop only).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if authentication succeeded</returns>
    Task<bool> AuthenticateWithDeviceCodeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Microsoft Graph client for API calls.
    /// </summary>
    /// <returns>GraphServiceClient or null if not authenticated</returns>
    Microsoft.Graph.GraphServiceClient? GetClient();

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    void SignOut();
}

