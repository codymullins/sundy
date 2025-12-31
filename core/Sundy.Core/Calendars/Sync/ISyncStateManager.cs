namespace Sundy.Core.Calendars.Sync;

/// <summary>
/// Manages observable sync state for calendars and provides activity logging.
/// UI components can subscribe to OnStateChanged to receive real-time updates.
/// </summary>
public interface ISyncStateManager
{
    /// <summary>
    /// Raised when any sync state changes (calendar states or logs).
    /// UI components should subscribe to this to trigger re-renders.
    /// </summary>
    event Action? OnStateChanged;

    /// <summary>
    /// Gets the current sync state for a specific calendar.
    /// Returns Idle state if calendar has never synced.
    /// </summary>
    CalendarSyncState GetCalendarState(string calendarId);

    /// <summary>
    /// Gets sync states for all tracked calendars.
    /// </summary>
    IReadOnlyDictionary<string, CalendarSyncState> GetAllCalendarStates();

    /// <summary>
    /// Updates the sync state for a calendar. Raises OnStateChanged.
    /// </summary>
    void UpdateCalendarState(string calendarId, CalendarSyncState state);

    /// <summary>
    /// Clears sync state for a calendar (e.g., when disconnected).
    /// </summary>
    void ClearCalendarState(string calendarId);

    /// <summary>
    /// Gets recent sync log entries, newest first.
    /// </summary>
    IReadOnlyList<SyncLogEntry> GetRecentLogs(int count = 50);

    /// <summary>
    /// Adds a new log entry. Raises OnStateChanged.
    /// </summary>
    void AddLogEntry(SyncLogEntry entry);

    /// <summary>
    /// Clears all log entries. Raises OnStateChanged.
    /// </summary>
    void ClearLogs();

    /// <summary>
    /// Returns true if any calendar is currently syncing.
    /// </summary>
    bool IsAnySyncing { get; }

    /// <summary>
    /// Returns the most recent sync time across all calendars.
    /// </summary>
    DateTime? LastGlobalSyncTime { get; }
}
