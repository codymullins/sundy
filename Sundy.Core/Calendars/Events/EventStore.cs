using Microsoft.EntityFrameworkCore;

namespace Sundy.Core;

public class EventStore(SundyDbContext dbContext)
{
    public async Task<List<CalendarEvent>> GetEventsInRangeAsync(DateTimeOffset startTime, DateTimeOffset endTime, string? calendarId = null, CancellationToken ct = default)
    {
        var query = dbContext.Events
            .AsNoTracking()
            .Where(e => e.StartTime < endTime && e.EndTime > startTime);

        if (!string.IsNullOrEmpty(calendarId))
        {
            query = query.Where(e => e.CalendarId == calendarId);
        }

        return await query.ToListAsync(ct).ConfigureAwait(false);
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
        dbContext.Events.Update(evt);
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

