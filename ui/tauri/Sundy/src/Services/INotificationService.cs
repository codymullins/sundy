namespace Sundy.Services;

/// <summary>
/// Cross-platform notification service.
/// - In Tauri mode: Syncs events to Rust backend for native OS notifications
/// - In browser mode: Uses Web Notifications API
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Whether running in Tauri (native notifications via Rust) or browser (Web Notifications API).
    /// </summary>
    Task<bool> IsTauriAsync();

    /// <summary>
    /// Whether notifications are supported on this platform.
    /// </summary>
    Task<bool> IsSupportedAsync();

    /// <summary>
    /// Gets the current notification permission status.
    /// Returns "granted", "denied", "default", or "unsupported".
    /// </summary>
    Task<string> GetPermissionAsync();

    /// <summary>
    /// Requests notification permission from the user.
    /// Returns "granted", "denied", or "unsupported".
    /// </summary>
    Task<string> RequestPermissionAsync();

    /// <summary>
    /// Syncs an event to the notification backend for reminder scheduling.
    /// In Tauri mode, this syncs to the Rust backend.
    /// In browser mode, this is handled by the ReminderScheduler.
    /// </summary>
    Task SyncEventAsync(string id, string title, DateTimeOffset startTime, int reminderMinutes);

    /// <summary>
    /// Removes an event from the notification backend.
    /// </summary>
    Task DeleteEventAsync(string id);

    /// <summary>
    /// Sends an immediate notification (browser mode only).
    /// In Tauri mode, notifications are handled by the Rust backend.
    /// </summary>
    Task SendBrowserNotificationAsync(string title, string body);

    /// <summary>
    /// Sends a reminder notification (browser mode only).
    /// </summary>
    Task SendReminderNotificationAsync(string eventTitle, DateTimeOffset eventTime, int minutesBefore, string eventId);

    /// <summary>
    /// Syncs all upcoming events to the notification backend.
    /// Call this at startup and after Outlook sync completes.
    /// </summary>
    Task SyncAllUpcomingEventsAsync(IEnumerable<Sundy.Core.CalendarEvent> events, int defaultReminderMinutes);

    /// <summary>
    /// Sends a test notification to verify the notification system is working.
    /// Works in both Tauri and browser modes.
    /// </summary>
    Task SendTestNotificationAsync();

    // =========================================================================
    // Persistent Notification Window (Tauri only)
    // =========================================================================

    /// <summary>
    /// Gets the user's notification display preference.
    /// Returns "os_only", "window_only", or "both".
    /// </summary>
    Task<string> GetNotificationPreferenceAsync();

    /// <summary>
    /// Sets the user's notification display preference.
    /// </summary>
    /// <param name="preference">"os_only", "window_only", or "both"</param>
    Task SetNotificationPreferenceAsync(string preference);

    /// <summary>
    /// Shows the persistent notification window (Tauri only).
    /// </summary>
    Task ShowNotificationWindowAsync();

    /// <summary>
    /// Hides the persistent notification window (Tauri only).
    /// </summary>
    Task HideNotificationWindowAsync();

    /// <summary>
    /// Adds a notification to the persistent window (Tauri only).
    /// </summary>
    /// <returns>The notification ID, or null if failed.</returns>
    Task<string?> AddPersistentNotificationAsync(string eventId, string title, string body);

    /// <summary>
    /// Dismisses a notification from the persistent window (Tauri only).
    /// </summary>
    Task DismissPersistentNotificationAsync(string notificationId);
}
