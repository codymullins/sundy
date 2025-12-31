namespace Sundy.Core.Calendars.Sync;

/// <summary>
/// Background service for syncing Outlook calendars.
/// Runs on a configurable timer and updates local events from Microsoft Graph.
/// </summary>
public interface IOutlookSyncService
{
    /// <summary>
    /// Whether the sync service is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets or sets the sync interval in minutes.
    /// Changes take effect on the next sync cycle.
    /// </summary>
    int SyncIntervalMinutes { get; set; }

    /// <summary>
    /// Starts the background sync timer.
    /// </summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Stops the background sync timer.
    /// </summary>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>
    /// Triggers an immediate sync for all calendars or a specific calendar.
    /// </summary>
    /// <param name="calendarId">Optional calendar ID to sync. If null, syncs all external calendars.</param>
    Task SyncNowAsync(string? calendarId = null, CancellationToken ct = default);

    /// <summary>
    /// Raised when the sync interval changes.
    /// </summary>
    event Action<int>? OnIntervalChanged;
}
