namespace Sundy.Core;

public interface ICalendarProvider
{
    Task<CalendarEvent> CreateEventAsync(string calendarId, CalendarEvent evt);
    Task<CalendarEvent> UpdateEventAsync(string calendarId, CalendarEvent evt);
    Task DeleteEventAsync(string calendarId, string eventId);
    Task<List<CalendarEvent>> GetEventsAsync(string calendarId, DateTime start, DateTime end);
}