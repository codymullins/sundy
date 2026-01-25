namespace Sundy.Core.Settings;

/// <summary>
/// Well-known setting keys used throughout the application.
/// </summary>
public static class SettingKeys
{
    /// <summary>
    /// Outlook sync interval in minutes (1, 5, 15, 30).
    /// </summary>
    public const string SyncIntervalMinutes = "sync.outlook.interval_minutes";

    /// <summary>
    /// Whether to show the status bar at the bottom of the screen.
    /// </summary>
    public const string ShowStatusBar = "ui.show_status_bar";

    /// <summary>
    /// Timestamp of the last sync attempt.
    /// </summary>
    public const string LastSyncAttempt = "sync.outlook.last_attempt";

    /// <summary>
    /// Whether privacy mode is enabled (master toggle for privacy features).
    /// </summary>
    public const string PrivacyMode = "ui.privacy_mode";

    /// <summary>
    /// Whether to hide email addresses in the UI when privacy mode is enabled.
    /// </summary>
    public const string PrivacyHideEmails = "privacy.hide_emails";

    /// <summary>
    /// Whether to hide event titles in the UI when privacy mode is enabled.
    /// </summary>
    public const string PrivacyHideEventTitles = "privacy.hide_event_titles";

    /// <summary>
    /// Whether to collapse past events in month view (show "X events" instead of badges).
    /// </summary>
    public const string CollapsePastEvents = "ui.collapse_past_events_month";

    /// <summary>
    /// Whether the Dynamic view is enabled (feature flag, defaults to false).
    /// </summary>
    public const string DynamicViewEnabled = "feature.dynamic_view_enabled";

    // Backup settings

    /// <summary>
    /// Whether automatic backups are enabled.
    /// </summary>
    public const string AutoBackupEnabled = "backup.auto_enabled";

    /// <summary>
    /// Automatic backup interval in days (1, 7, 30).
    /// </summary>
    public const string BackupIntervalDays = "backup.interval_days";

    /// <summary>
    /// Number of backups to keep (retention policy).
    /// </summary>
    public const string BackupRetentionCount = "backup.retention_count";

    /// <summary>
    /// Timestamp of the last automatic backup.
    /// </summary>
    public const string LastBackupAt = "backup.last_backup_at";

    // Privacy settings

    /// <summary>
    /// Whether telemetry (error reporting, tracing) is enabled. Defaults to false (opt-in).
    /// </summary>
    public const string TelemetryEnabled = "privacy.telemetry_enabled";

    // Demo mode settings

    /// <summary>
    /// Whether demo mode is enabled. In demo mode, telemetry is enabled by default.
    /// </summary>
    public const string DemoMode = "app.demo_mode";

    /// <summary>
    /// Whether the demo mode banner has been dismissed by the user.
    /// </summary>
    public const string DemoBannerDismissed = "ui.demo_banner_dismissed";

    // Notification/Reminder settings

    /// <summary>
    /// Whether event reminders are enabled. Defaults to true.
    /// </summary>
    public const string RemindersEnabled = "notifications.reminders_enabled";

    /// <summary>
    /// Default reminder time in minutes before events. Defaults to 15.
    /// </summary>
    public const string DefaultReminderMinutes = "notifications.default_reminder_minutes";
}
