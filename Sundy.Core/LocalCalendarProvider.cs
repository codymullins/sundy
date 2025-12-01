using Microsoft.EntityFrameworkCore;

namespace Sundy.Core;

public class LocalCalendarProvider(SundyDbContext db) : ICalendarProvider
{
    public async Task<CalendarEvent> CreateEventAsync(string calendarId, CalendarEvent evt, CancellationToken ct = default)
    {
        evt.Id = Guid.NewGuid().ToString();
        evt.CalendarId = calendarId;
        db.Events.Add(evt);
        await db.SaveChangesAsync(ct);
        return evt;
    }

    public async Task<CalendarEvent> UpdateEventAsync(string calendarId, CalendarEvent evt, CancellationToken ct = default)
    {
        var existing = await db.Events.Where(p => p.CalendarId == calendarId && p.Id == evt.Id).SingleOrDefaultAsync(ct);
        if (existing == null)
        {
            throw new EventNotFoundException(evt.Id);
        }

        existing.Title = evt.Title;
        existing.StartTime = evt.StartTime;
        existing.EndTime = evt.EndTime;
        existing.Description = evt.Description;
        existing.Location = evt.Location;

        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task DeleteEventAsync(string calendarId, string eventId, CancellationToken ct = default)
    {
        var evt = await db.Events
            .Where(p => p.CalendarId == calendarId && p.Id == eventId)
            .SingleOrDefaultAsync(ct);
        
        if (evt != null)
        {
            db.Events.Remove(evt);
            await db.SaveChangesAsync(ct);
        }
    }

    // TODO: async enumerable for large datasets
    public async Task<List<CalendarEvent>> GetEventsAsync(
        string calendarId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken ct = default)
    {
        return await db.Events.ToListAsync(ct);
            // TODO: Re-enable filtering
            // .Where(e => e.CalendarId == calendarId
            //             && e.StartTime < end
            //             && e.EndTime > start)
            // .ToListAsync(ct);
    }
}