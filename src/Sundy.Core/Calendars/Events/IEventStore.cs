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

    Task<CalendarEvent> CreateEventAsync(CalendarEvent evt, CancellationToken ct = default);

    Task UpdateEventAsync(CalendarEvent evt, CancellationToken ct = default);

    Task DeleteEventAsync(string eventId, CancellationToken ct = default);
}
