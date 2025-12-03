using Microsoft.EntityFrameworkCore;

namespace Sundy.Core;

public class CalendarStore(SundyDbContext dbContext)
{
    public async Task DeleteCalendarAsync(string calendarId, CancellationToken ct = default)
    {
        var calendar = await dbContext.Calendars.FindAsync([calendarId], ct).ConfigureAwait(false);
        if (calendar != null)
        {
            dbContext.Calendars.Remove(calendar);
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }

    public async Task<List<Calendar>> GetAllCalendarsAsync(CancellationToken ct = default)
    {
        return await dbContext.Calendars
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task CreateCalendarAsync(Calendar calendar, CancellationToken ct = default)
    {
        dbContext.Calendars.Add(calendar);
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<Dictionary<string, Calendar>> GetCalendarLookupAsync(CancellationToken ct = default)
    {
        return await dbContext.Calendars
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, ct)
            .ConfigureAwait(false);
    }
}