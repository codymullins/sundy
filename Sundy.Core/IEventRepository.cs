namespace Sundy.Core;

public interface IEventRepository
{
    Task InitializeDatabaseAsync(CancellationToken ct = default);
    Task<List<CalendarEvent>> GetEventsInRangeAsync(DateTimeOffset startTime, DateTimeOffset endTime, string? calendarId = null, CancellationToken ct = default);
    Task<CalendarEvent?> GetEventByIdAsync(string eventId, CancellationToken ct = default);
    Task<CalendarEvent> CreateEventAsync(CalendarEvent evt, CancellationToken ct = default);
    Task UpdateEventAsync(CalendarEvent evt, CancellationToken ct = default);
    Task DeleteEventAsync(string eventId, CancellationToken ct = default);
    Task<Dictionary<string, Calendar>> GetCalendarLookupAsync(CancellationToken ct = default);
    Task DeleteCalendarAsync(string calendarId, CancellationToken ct = default);
    
    Task<List<Calendar>> GetAllCalendarsAsync(CancellationToken ct = default);
    Task CreateCalendarAsync(Calendar calendar, CancellationToken ct = default);
    Task ResetDatabaseAsync(CancellationToken ct = default);
}

