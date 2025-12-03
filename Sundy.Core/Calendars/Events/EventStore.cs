using System.Data;
using Microsoft.Data.Sqlite;

namespace Sundy.Core;

public class EventStore(string connectionString)
{
    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }

    public async Task<List<CalendarEvent>> GetEventsInRangeAsync(DateTimeOffset startTime, DateTimeOffset endTime, string? calendarId = null, CancellationToken ct = default)
    {
        var events = new List<CalendarEvent>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var sql = @"
            SELECT Id, CalendarId, Title, StartTime, EndTime, Description, Location, IsBlockingEvent, SourceEventId
            FROM Events
            WHERE StartTime < @EndTime AND EndTime > @StartTime";

        if (!string.IsNullOrEmpty(calendarId))
        {
            sql += " AND CalendarId = @CalendarId";
        }

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@StartTime", startTime.UtcTicks);
        command.Parameters.AddWithValue("@EndTime", endTime.UtcTicks);

        if (!string.IsNullOrEmpty(calendarId))
        {
            command.Parameters.AddWithValue("@CalendarId", calendarId);
        }

        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            events.Add(MapEvent(reader));
        }

        return events;
    }

    public async Task<CalendarEvent?> GetEventByIdAsync(string eventId, CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var sql = @"
            SELECT Id, CalendarId, Title, StartTime, EndTime, Description, Location, IsBlockingEvent, SourceEventId
            FROM Events
            WHERE Id = @Id";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", eventId);

        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return MapEvent(reader);
        }

        return null;
    }

    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO Events (Id, CalendarId, Title, StartTime, EndTime, Description, Location, IsBlockingEvent, SourceEventId)
            VALUES (@Id, @CalendarId, @Title, @StartTime, @EndTime, @Description, @Location, @IsBlockingEvent, @SourceEventId)";

        await using var command = new SqliteCommand(sql, connection);
        AddEventParameters(command, evt);

        await command.ExecuteNonQueryAsync();

        return evt;
    }

    public async Task UpdateEventAsync(CalendarEvent evt, CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            UPDATE Events
            SET CalendarId = @CalendarId,
                Title = @Title,
                StartTime = @StartTime,
                EndTime = @EndTime,
                Description = @Description,
                Location = @Location,
                IsBlockingEvent = @IsBlockingEvent,
                SourceEventId = @SourceEventId
            WHERE Id = @Id";

        await using var command = new SqliteCommand(sql, connection);
        AddEventParameters(command, evt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteEventAsync(string eventId, CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var sql = "DELETE FROM Events WHERE Id = @Id";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", eventId);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static CalendarEvent MapEvent(SqliteDataReader reader)
    {
        return new CalendarEvent
        {
            Id = reader.GetString(0),
            CalendarId = reader.GetString(1),
            Title = reader.IsDBNull(2) ? null : reader.GetString(2),
            StartTime = new DateTimeOffset(reader.GetInt64(3), TimeSpan.Zero),
            EndTime = new DateTimeOffset(reader.GetInt64(4), TimeSpan.Zero),
            Description = reader.IsDBNull(5) ? null : reader.GetString(5),
            Location = reader.IsDBNull(6) ? null : reader.GetString(6),
            IsBlockingEvent = reader.GetBoolean(7),
            SourceEventId = reader.IsDBNull(8) ? null : reader.GetString(8)
        };
    }

    private static void AddEventParameters(SqliteCommand command, CalendarEvent evt)
    {
        command.Parameters.AddWithValue("@Id", evt.Id ?? Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("@CalendarId", evt.CalendarId ?? string.Empty);
        command.Parameters.AddWithValue("@Title", (object?)evt.Title ?? DBNull.Value);
        command.Parameters.AddWithValue("@StartTime", evt.StartTime.UtcTicks);
        command.Parameters.AddWithValue("@EndTime", evt.EndTime.UtcTicks);
        command.Parameters.AddWithValue("@Description", (object?)evt.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Location", (object?)evt.Location ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsBlockingEvent", evt.IsBlockingEvent);
        command.Parameters.AddWithValue("@SourceEventId", (object?)evt.SourceEventId ?? DBNull.Value);
    }
}

