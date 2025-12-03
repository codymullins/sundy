namespace Sundy.Core;

public interface ICalendarProvider
{
    Task<CalendarEvent> CreateEventAsync(string calendarId, CalendarEvent evt, CancellationToken ct = default);
    Task<CalendarEvent> UpdateEventAsync(string calendarId, CalendarEvent evt, CancellationToken ct = default);
    Task DeleteEventAsync(string calendarId, string eventId, CancellationToken ct = default);
    Task<List<CalendarEvent>> GetEventsAsync(string calendarId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct = default);
}