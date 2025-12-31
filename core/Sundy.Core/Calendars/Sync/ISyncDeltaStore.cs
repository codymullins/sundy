namespace Sundy.Core.Calendars.Sync;

/// <summary>
/// Stores Microsoft Graph delta tokens for incremental calendar sync.
/// Each external calendar has its own delta token for tracking changes.
/// </summary>
public interface ISyncDeltaStore
{
    /// <summary>
    /// Gets the delta token for a calendar.
    /// Returns null if no token exists (requires full sync).
    /// </summary>
    Task<string?> GetDeltaTokenAsync(string calendarId, CancellationToken ct = default);

    /// <summary>
    /// Saves or updates the delta token for a calendar.
    /// </summary>
    Task SaveDeltaTokenAsync(string calendarId, string deltaToken, CancellationToken ct = default);

    /// <summary>
    /// Clears the delta token for a calendar (forces full sync on next run).
    /// </summary>
    Task ClearDeltaTokenAsync(string calendarId, CancellationToken ct = default);

    /// <summary>
    /// Clears all delta tokens (e.g., on account disconnect).
    /// </summary>
    Task ClearAllAsync(CancellationToken ct = default);
}
