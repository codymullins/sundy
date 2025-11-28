using Microsoft.EntityFrameworkCore;

namespace Sundy.Core;

public class LocalCalendarProvider : ICalendarProvider
{
    private readonly SundyDbContext _db;
    
    public async Task<CalendarEvent> CreateEventAsync(string calendarId, CalendarEvent evt)
    {
        evt.Id = Guid.NewGuid().ToString();
        evt.CalendarId = calendarId;
        _db.Events.Add(evt);
        await _db.SaveChangesAsync();
        return evt;
    }
    
    public async Task<CalendarEvent> UpdateEventAsync(string calendarId, CalendarEvent evt)
    {
        var existing = await _db.Events.FindAsync(evt.Id);
        if (existing == null) throw new Exception("Event not found");
        
        existing.Title = evt.Title;
        existing.StartTime = evt.StartTime;
        existing.EndTime = evt.EndTime;
        existing.Description = evt.Description;
        existing.Location = evt.Location;
        
        await _db.SaveChangesAsync();
        return existing;
    }
    
    public async Task DeleteEventAsync(string calendarId, string eventId)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt != null)
        {
            _db.Events.Remove(evt);
            await _db.SaveChangesAsync();
        }
    }
    
    public async Task<List<CalendarEvent>> GetEventsAsync(
        string calendarId, 
        DateTime start, 
        DateTime end)
    {
        return await _db.Events
            .Where(e => e.CalendarId == calendarId 
                        && e.StartTime < end 
                        && e.EndTime > start)
            .ToListAsync();
    }
}