using System.Data;
using System.Text;
using Dapper;

namespace Sundy.Core;

public class DapperEventStore(IDbConnection connection) : IEventStore
{
    public async Task<List<CalendarEvent>> GetEventsInRangeAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string? calendarId = null,
        IReadOnlyList<string>? visibleCalendarIds = null,
        CancellationToken ct = default)
    {
        // DateTimeOffset is stored as ISO8601 string, which sorts correctly lexicographically
        var sql = new StringBuilder("""
            SELECT Id, CalendarId, Title, StartTime, EndTime, Description, Location, IsBlockingEvent, SourceEventId
            FROM Events
            WHERE StartTime < @EndTime AND EndTime > @StartTime
            """);

        var parameters = new DynamicParameters();
        parameters.Add("StartTime", startTime.ToString("o"));
        parameters.Add("EndTime", endTime.ToString("o"));

        if (!string.IsNullOrEmpty(calendarId))
        {
            sql.Append(" AND CalendarId = @CalendarId");
            parameters.Add("CalendarId", calendarId);
        }

        var command = new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct);
        var results = await connection.QueryAsync<EventDto>(command).ConfigureAwait(false);
        var events = results.Select(MapFromDto).ToList();

        // Filter by visible calendar IDs in memory (matching EF Core behavior)
        if (visibleCalendarIds is not null)
        {
            var ids = new HashSet<string>(visibleCalendarIds);
            events = events.Where(e => e.CalendarId is not null && ids.Contains(e.CalendarId)).ToList();
        }

        return events;
    }

    public async Task<CalendarEvent?> GetEventByIdAsync(string eventId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CalendarId, Title, StartTime, EndTime, Description, Location, IsBlockingEvent, SourceEventId
            FROM Events
            WHERE Id = @Id
            """;
        var command = new CommandDefinition(sql, new { Id = eventId }, cancellationToken: ct);
        var dto = await connection.QueryFirstOrDefaultAsync<EventDto>(command).ConfigureAwait(false);
        return dto is null ? null : MapFromDto(dto);
    }

    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(evt.Id))
        {
            evt.Id = Guid.NewGuid().ToString();
        }

        const string sql = """
            INSERT INTO Events (Id, CalendarId, Title, StartTime, EndTime, Description, Location, IsBlockingEvent, SourceEventId)
            VALUES (@Id, @CalendarId, @Title, @StartTime, @EndTime, @Description, @Location, @IsBlockingEvent, @SourceEventId)
            """;
        var command = new CommandDefinition(sql, new
        {
            evt.Id,
            evt.CalendarId,
            evt.Title,
            StartTime = evt.StartTime.ToString("o"),
            EndTime = evt.EndTime.ToString("o"),
            evt.Description,
            evt.Location,
            IsBlockingEvent = evt.IsBlockingEvent ? 1 : 0,
            evt.SourceEventId
        }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);

        return evt;
    }

    public async Task UpdateEventAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        // First check if the event exists (matching EF Core behavior)
        var existing = await GetEventByIdAsync(evt.Id!, ct).ConfigureAwait(false);
        if (existing is null)
        {
            throw new InvalidOperationException($"Event with ID {evt.Id} not found.");
        }

        const string sql = """
            UPDATE Events
            SET Title = @Title,
                StartTime = @StartTime,
                EndTime = @EndTime,
                Location = @Location,
                Description = @Description,
                CalendarId = @CalendarId,
                IsBlockingEvent = @IsBlockingEvent
            WHERE Id = @Id
            """;
        var command = new CommandDefinition(sql, new
        {
            evt.Id,
            evt.Title,
            StartTime = evt.StartTime.ToString("o"),
            EndTime = evt.EndTime.ToString("o"),
            evt.Location,
            evt.Description,
            evt.CalendarId,
            IsBlockingEvent = evt.IsBlockingEvent ? 1 : 0
        }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public async Task DeleteEventAsync(string eventId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM Events WHERE Id = @Id";
        var command = new CommandDefinition(sql, new { Id = eventId }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    private static CalendarEvent MapFromDto(EventDto dto) => new()
    {
        Id = dto.Id,
        CalendarId = dto.CalendarId,
        Title = dto.Title,
        StartTime = DateTimeOffset.Parse(dto.StartTime),
        EndTime = DateTimeOffset.Parse(dto.EndTime),
        Description = dto.Description,
        Location = dto.Location,
        IsBlockingEvent = dto.IsBlockingEvent,
        SourceEventId = dto.SourceEventId
    };

    // DTO for Dapper mapping since DateTimeOffset is stored as ISO8601 string
    private sealed class EventDto
    {
        public string? Id { get; set; }
        public string? CalendarId { get; set; }
        public string? Title { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool IsBlockingEvent { get; set; }
        public string? SourceEventId { get; set; }
    }
}
