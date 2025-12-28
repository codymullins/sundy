namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Event args containing device code authentication information.
/// </summary>
public class DeviceCodeInfo : EventArgs
{
    /// <summary>
    /// The URL the user should visit to authenticate.
    /// Example: "https://microsoft.com/devicelogin"
    /// </summary>
    public required string VerificationUrl { get; init; }

    /// <summary>
    /// The code the user should enter at the verification URL.
    /// Example: "ABCD1234"
    /// </summary>
    public required string UserCode { get; init; }

    /// <summary>
    /// The full message from the device code result (for fallback display).
    /// </summary>
    public required string FullMessage { get; init; }

    /// <summary>
    /// When the device code expires.
    /// </summary>
    public DateTimeOffset ExpiresOn { get; init; }
}
