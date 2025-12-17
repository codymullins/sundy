using Microsoft.EntityFrameworkCore;

namespace Sundy.Core;

public class SQLiteEventStore(SundyDbContext dbContext) : IEventStore
{
    public async Task<List<CalendarEvent>> GetEventsInRangeAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string? calendarId = null,
        IReadOnlyList<string>? visibleCalendarIds = null,
        CancellationToken ct = default)
    {
        var query = dbContext.Events
            .AsNoTracking()
            .Where(e => e.StartTime < endTime && e.EndTime > startTime);

        if (!string.IsNullOrEmpty(calendarId))
        {
            query = query.Where(e => e.CalendarId == calendarId);
        }

        var results = await query.ToListAsync(ct).ConfigureAwait(false);

        if (visibleCalendarIds is not null)
        {
            var ids = new HashSet<string>(visibleCalendarIds);
            results = results.Where(e => ids.Contains(e.CalendarId)).ToList();
        }

        return results;
    }

    public async Task<CalendarEvent?> GetEventByIdAsync(string eventId, CancellationToken ct = default)
    {
        return await dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, ct)
            .ConfigureAwait(false);
    }

    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(evt.Id))
        {
            evt.Id = Guid.NewGuid().ToString();
        }

        dbContext.Events.Add(evt);
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);

        return evt;
    }

    public async Task UpdateEventAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        var existing = await dbContext.Events.FindAsync([evt.Id], ct).ConfigureAwait(false);
        if (existing == null)
        {
            throw new InvalidOperationException($"Event with ID {evt.Id} not found.");
        }
        
        existing.Title = evt.Title;
        existing.StartTime = evt.StartTime;
        existing.EndTime = evt.EndTime;
        existing.Location = evt.Location;
        existing.Description = evt.Description;
        existing.CalendarId = evt.CalendarId;
        existing.IsBlockingEvent = evt.IsBlockingEvent;
        
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task DeleteEventAsync(string eventId, CancellationToken ct = default)
    {
        var evt = await dbContext.Events.FindAsync([eventId], ct).ConfigureAwait(false);
        if (evt != null)
        {
            dbContext.Events.Remove(evt);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }
}

