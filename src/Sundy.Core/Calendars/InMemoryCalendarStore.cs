using System.Collections.Concurrent;

namespace Sundy.Core;

public interface ICalendarStore
{
    Task DeleteCalendarAsync(string calendarId, CancellationToken ct = default);
    Task<List<Calendar>> GetAllAsync(CancellationToken ct = default);
    Task CreateCalendarAsync(Calendar calendar, CancellationToken ct = default);

    /// <summary>
    /// Alias for CreateCalendarAsync for convenience.
    /// </summary>
    Task AddAsync(Calendar calendar, CancellationToken ct = default);

    Task<Dictionary<string, Calendar>> GetCalendarLookupAsync(CancellationToken ct = default);
}

public class InMemoryCalendarStore : ICalendarStore
{
    private readonly ConcurrentDictionary<string, Calendar> _calendars = new();

    public Task DeleteCalendarAsync(string calendarId, CancellationToken ct = default)
    {
        _calendars.TryRemove(calendarId, out _);
        return Task.CompletedTask;
    }

    public Task<List<Calendar>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_calendars.Values.ToList());
    }

    public Task CreateCalendarAsync(Calendar calendar, CancellationToken ct = default)
    {
        _calendars[calendar.Id] = calendar;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Alias for CreateCalendarAsync for convenience.
    /// </summary>
    public Task AddAsync(Calendar calendar, CancellationToken ct = default)
        => CreateCalendarAsync(calendar, ct);

    public Task<Dictionary<string, Calendar>> GetCalendarLookupAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new Dictionary<string, Calendar>(_calendars));
    }
}
