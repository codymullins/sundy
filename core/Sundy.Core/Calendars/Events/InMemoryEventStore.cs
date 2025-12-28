using System.Collections.Concurrent;

namespace Sundy.Core;

public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentDictionary<string, CalendarEvent> _events = new();

    public Task<List<CalendarEvent>> GetEventsInRangeAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string? calendarId = null,
        IReadOnlyList<string>? visibleCalendarIds = null,
        CancellationToken ct = default)
    {
        var query = _events.Values
            .Where(e => e.StartTime < endTime && e.EndTime > startTime);

        if (!string.IsNullOrEmpty(calendarId))
        {
            query = query.Where(e => e.CalendarId == calendarId);
        }

        var results = query.ToList();

        if (visibleCalendarIds is not null)
        {
            var ids = new HashSet<string>(visibleCalendarIds);
            results = results.Where(e => e.CalendarId != null && ids.Contains(e.CalendarId)).ToList();
        }

        return Task.FromResult(results);
    }

    public Task<CalendarEvent?> GetEventByIdAsync(string eventId, CancellationToken ct = default)
    {
        _events.TryGetValue(eventId, out var evt);
        return Task.FromResult(evt);
    }

    public Task<CalendarEvent> CreateEventAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(evt.Id))
        {
            evt.Id = Guid.NewGuid().ToString();
        }

        _events[evt.Id] = evt;
        return Task.FromResult(evt);
    }

    public Task UpdateEventAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        if (!_events.TryGetValue(evt.Id ?? string.Empty, out var existing))
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

        return Task.CompletedTask;
    }

    public Task DeleteEventAsync(string eventId, CancellationToken ct = default)
    {
        _events.TryRemove(eventId, out _);
        return Task.CompletedTask;
    }
}
