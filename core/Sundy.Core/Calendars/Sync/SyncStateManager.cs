using System.Collections.Concurrent;

namespace Sundy.Core.Calendars.Sync;

/// <summary>
/// In-memory implementation of sync state management.
/// Thread-safe for use with background sync operations.
/// </summary>
public class SyncStateManager : ISyncStateManager
{
    private const int MaxLogEntries = 200;

    private readonly ConcurrentDictionary<string, CalendarSyncState> _calendarStates = new();
    private readonly LinkedList<SyncLogEntry> _logEntries = new();
    private readonly object _logLock = new();

    public event Action? OnStateChanged;

    public CalendarSyncState GetCalendarState(string calendarId)
    {
        return _calendarStates.TryGetValue(calendarId, out var state)
            ? state
            : new CalendarSyncState { CalendarId = calendarId, Status = SyncStatus.Idle };
    }

    public IReadOnlyDictionary<string, CalendarSyncState> GetAllCalendarStates()
    {
        return _calendarStates;
    }

    public void UpdateCalendarState(string calendarId, CalendarSyncState state)
    {
        _calendarStates[calendarId] = state with { CalendarId = calendarId };
        RaiseStateChanged();
    }

    public void ClearCalendarState(string calendarId)
    {
        _calendarStates.TryRemove(calendarId, out _);
        RaiseStateChanged();
    }

    public IReadOnlyList<SyncLogEntry> GetRecentLogs(int count = 50)
    {
        lock (_logLock)
        {
            return _logEntries.Take(count).ToList();
        }
    }

    public void AddLogEntry(SyncLogEntry entry)
    {
        lock (_logLock)
        {
            // Add to front (newest first)
            _logEntries.AddFirst(entry);

            // Trim if exceeds max
            while (_logEntries.Count > MaxLogEntries)
            {
                _logEntries.RemoveLast();
            }
        }

        RaiseStateChanged();
    }

    public void ClearLogs()
    {
        lock (_logLock)
        {
            _logEntries.Clear();
        }

        RaiseStateChanged();
    }

    public bool IsAnySyncing =>
        _calendarStates.Values.Any(s => s.Status == SyncStatus.Syncing);

    public DateTime? LastGlobalSyncTime =>
        _calendarStates.Values
            .Where(s => s.LastSyncTime.HasValue)
            .Select(s => s.LastSyncTime!.Value)
            .OrderByDescending(t => t)
            .FirstOrDefault();

    private void RaiseStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
