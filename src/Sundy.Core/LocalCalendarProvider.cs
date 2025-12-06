
namespace Sundy.Core;

public class LocalCalendarProvider(EventStore eventStore) : ICalendarProvider
{
    public async Task<CalendarEvent> CreateEventAsync(string calendarId, CalendarEvent evt, CancellationToken ct = default)
    {
        evt.Id = Guid.NewGuid().ToString();
        evt.CalendarId = calendarId;
        await eventStore.CreateEventAsync(evt, ct).ConfigureAwait(false);
        return evt;
    }

    public async Task<CalendarEvent> UpdateEventAsync(string calendarId, CalendarEvent evt, CancellationToken ct = default)
    {
        var existing = await eventStore.GetEventByIdAsync(evt.Id, ct).ConfigureAwait(false);
        if (existing == null)
        {
            throw new EventNotFoundException(evt.Id);
        }

        existing.Title = evt.Title;
        existing.StartTime = evt.StartTime;
        existing.EndTime = evt.EndTime;
        existing.Description = evt.Description;
        existing.Location = evt.Location;
        
        await eventStore.UpdateEventAsync(existing, ct).ConfigureAwait(false);

        return existing;
    }

    public async Task DeleteEventAsync(string calendarId, string eventId, CancellationToken ct = default)
    {
        await eventStore.DeleteEventAsync(eventId, ct).ConfigureAwait(false);
    }

    // TODO: async enumerable for large datasets
    public async Task<List<CalendarEvent>> GetEventsAsync(
        string calendarId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken ct = default)
    {
        return await eventStore.GetEventsInRangeAsync(start, end, calendarId, visibleCalendarIds: null, ct).ConfigureAwait(false);
    }
}
