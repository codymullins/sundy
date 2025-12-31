namespace Sundy.Core.Calendars.Sync;

/// <summary>
/// Represents the current sync status of a calendar.
/// </summary>
public enum SyncStatus
{
    /// <summary>Calendar is not currently syncing.</summary>
    Idle,

    /// <summary>Calendar is actively syncing with external source.</summary>
    Syncing,

    /// <summary>Last sync completed successfully.</summary>
    Success,

    /// <summary>Last sync failed with an error.</summary>
    Error
}

/// <summary>
/// Represents the sync state of a single calendar.
/// </summary>
public record CalendarSyncState
{
    /// <summary>The calendar ID this state applies to.</summary>
    public string CalendarId { get; init; } = string.Empty;

    /// <summary>Current sync status.</summary>
    public SyncStatus Status { get; init; } = SyncStatus.Idle;

    /// <summary>When the last sync completed (success or failure).</summary>
    public DateTime? LastSyncTime { get; init; }

    /// <summary>Error message if status is Error.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Number of events added in last sync.</summary>
    public int EventsAdded { get; init; }

    /// <summary>Number of events updated in last sync.</summary>
    public int EventsUpdated { get; init; }

    /// <summary>Number of events deleted in last sync.</summary>
    public int EventsDeleted { get; init; }
}

/// <summary>
/// Log level for sync activity entries.
/// </summary>
public enum SyncLogLevel
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Represents a single entry in the sync activity log.
/// </summary>
public record SyncLogEntry
{
    /// <summary>When this log entry was created.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>The calendar ID this entry relates to (empty for global entries).</summary>
    public string CalendarId { get; init; } = string.Empty;

    /// <summary>Display name of the calendar for UI purposes.</summary>
    public string CalendarName { get; init; } = string.Empty;

    /// <summary>Severity level of this log entry.</summary>
    public SyncLogLevel Level { get; init; }

    /// <summary>Human-readable log message.</summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Result of a delta sync operation for a single calendar.
/// </summary>
public record DeltaSyncResult
{
    /// <summary>Events that were added or modified.</summary>
    public List<CalendarEvent> AddedOrModified { get; init; } = new();

    /// <summary>IDs of events that were deleted (Graph event IDs, not local).</summary>
    public List<string> DeletedEventIds { get; init; } = new();

    /// <summary>Delta token to use for the next sync request.</summary>
    public string? NextDeltaToken { get; init; }

    /// <summary>If true, a full sync is required (delta token expired or invalid).</summary>
    public bool RequiresFullSync { get; init; }
}
