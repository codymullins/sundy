namespace Sundy.Core.Settings;

/// <summary>
/// Provides typed access to application settings with SQLite persistence.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value by key. Returns null if not found.
    /// </summary>
    Task<string?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Gets a typed setting value. Returns default if not found or cannot parse.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a setting value.
    /// </summary>
    Task SetAsync(string key, string value, CancellationToken ct = default);

    /// <summary>
    /// Sets a typed setting value (serialized to string).
    /// </summary>
    Task SetAsync<T>(string key, T value, CancellationToken ct = default);

    /// <summary>
    /// Gets the Outlook sync interval in minutes. Defaults to 5.
    /// </summary>
    Task<int> GetSyncIntervalMinutesAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets the Outlook sync interval in minutes.
    /// </summary>
    Task SetSyncIntervalMinutesAsync(int minutes, CancellationToken ct = default);

    /// <summary>
    /// Gets whether the status bar should be shown. Defaults to false.
    /// </summary>
    Task<bool> GetShowStatusBarAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether the status bar should be shown.
    /// </summary>
    Task SetShowStatusBarAsync(bool show, CancellationToken ct = default);

    /// <summary>
    /// Gets whether privacy mode is enabled (master toggle). Defaults to false.
    /// </summary>
    Task<bool> GetPrivacyModeAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether privacy mode is enabled (master toggle).
    /// </summary>
    Task SetPrivacyModeAsync(bool enabled, CancellationToken ct = default);

    /// <summary>
    /// Gets whether to hide emails when privacy mode is enabled. Defaults to true.
    /// </summary>
    Task<bool> GetPrivacyHideEmailsAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether to hide emails when privacy mode is enabled.
    /// </summary>
    Task SetPrivacyHideEmailsAsync(bool enabled, CancellationToken ct = default);

    /// <summary>
    /// Gets whether to hide event titles when privacy mode is enabled. Defaults to false.
    /// </summary>
    Task<bool> GetPrivacyHideEventTitlesAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether to hide event titles when privacy mode is enabled.
    /// </summary>
    Task SetPrivacyHideEventTitlesAsync(bool enabled, CancellationToken ct = default);

    /// <summary>
    /// Gets whether past events should be collapsed in month view. Defaults to true.
    /// </summary>
    Task<bool> GetCollapsePastEventsAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether past events should be collapsed in month view.
    /// </summary>
    Task SetCollapsePastEventsAsync(bool enabled, CancellationToken ct = default);

    /// <summary>
    /// Gets whether the Dynamic view feature is enabled. Defaults to false.
    /// </summary>
    Task<bool> GetDynamicViewEnabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether the Dynamic view feature is enabled.
    /// </summary>
    Task SetDynamicViewEnabledAsync(bool enabled, CancellationToken ct = default);

    /// <summary>
    /// Gets whether telemetry is enabled. Defaults to false (opt-in).
    /// </summary>
    Task<bool> GetTelemetryEnabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether telemetry is enabled.
    /// </summary>
    Task SetTelemetryEnabledAsync(bool enabled, CancellationToken ct = default);

    /// <summary>
    /// Gets whether demo mode is enabled. Defaults to false.
    /// </summary>
    Task<bool> GetDemoModeAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether demo mode is enabled.
    /// </summary>
    Task SetDemoModeAsync(bool enabled, CancellationToken ct = default);

    /// <summary>
    /// Gets whether the demo mode banner has been dismissed. Defaults to false.
    /// </summary>
    Task<bool> GetDemoBannerDismissedAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets whether the demo mode banner has been dismissed.
    /// </summary>
    Task SetDemoBannerDismissedAsync(bool dismissed, CancellationToken ct = default);

    /// <summary>
    /// Raised when any setting changes.
    /// </summary>
    event Action<string>? OnSettingChanged;
}
