namespace Sundy.Core;

public interface IEventStore
{
    Task<List<CalendarEvent>> GetEventsInRangeAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string? calendarId = null,
        IReadOnlyList<string>? visibleCalendarIds = null,
        CancellationToken ct = default);

    Task<CalendarEvent?> GetEventByIdAsync(string eventId, CancellationToken ct = default);

    /// <summary>
    /// Gets an event by its external ID within a specific calendar.
    /// Used for syncing external calendar events.
    /// </summary>
    Task<CalendarEvent?> GetByExternalIdAsync(string calendarId, string externalId, CancellationToken ct = default);

    /// <summary>
    /// Gets all events for a specific calendar.
    /// </summary>
    Task<List<CalendarEvent>> GetByCalendarIdAsync(string calendarId, CancellationToken ct = default);

    Task<CalendarEvent> CreateEventAsync(CalendarEvent evt, CancellationToken ct = default);

    Task UpdateEventAsync(CalendarEvent evt, CancellationToken ct = default);

    Task DeleteEventAsync(string eventId, CancellationToken ct = default);

    /// <summary>
    /// Deletes all events for a specific calendar.
    /// Used when disconnecting an external calendar.
    /// </summary>
    Task DeleteByCalendarIdAsync(string calendarId, CancellationToken ct = default);
}
