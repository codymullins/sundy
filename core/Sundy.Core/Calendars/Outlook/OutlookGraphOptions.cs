namespace Sundy.Core.Calendars.Outlook;

public record OutlookGraphOptions
{
    public string? ClientId { get; set; } = "45770f2c-6da1-47f0-9ee0-16ac86df3a10";

    public string TenantId { get; set; } = "common";

    /// <summary>
    /// When true, uses device code flow for authentication.
    /// When false (default), uses interactive browser flow.
    /// </summary>
    public bool UseDeviceCodeFlow { get; set; } = false;

    /// <summary>
    /// When true, uses DefaultAzureCredential which supports Azure CLI, VS Code,
    /// Visual Studio, and other development-time credentials.
    /// When false (default), uses InteractiveBrowserCredential for production desktop apps.
    ///
    /// Currently, InteractiveBrowserCredential is not supported for MacOS 26.1
    /// </summary>
    public bool UseDevelopmentCredential { get; set; } = false;

    public string[]? GraphUserScopes { get; set; } =
    [
        // Request access to read/write all calendars available to the user
        "Calendars.ReadWrite",
        "User.Read"
    ];
}
