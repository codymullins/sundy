namespace Sundy.Services;

/// <summary>
/// Background service for scheduling and triggering calendar event reminders.
/// - In Tauri mode: Delegates to Rust backend (events are synced via INotificationService)
/// - In browser mode: Polls the event store and sends Web Notifications
/// </summary>
public interface IReminderScheduler
{
    /// <summary>
    /// Starts the reminder scheduler.
    /// In browser mode, this starts the polling timer.
    /// In Tauri mode, this is a no-op (Rust handles it).
    /// </summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Stops the reminder scheduler.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Whether the scheduler is running (browser mode only).
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Forces an immediate check for due reminders (browser mode only).
    /// </summary>
    Task CheckRemindersAsync(CancellationToken ct = default);
}
