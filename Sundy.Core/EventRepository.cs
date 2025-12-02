using System.Data;
using Microsoft.Data.Sqlite;

namespace Sundy.Core;

public class EventRepository(string connectionString) : IEventRepository
{
    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }

    public async Task InitializeDatabaseAsync(CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var createCalendarsTable = @"
            CREATE TABLE IF NOT EXISTS Calendars (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Color TEXT NOT NULL,
                Type INTEGER NOT NULL,
                EnableBlocking INTEGER NOT NULL,
                ReceiveBlocks INTEGER NOT NULL
            )";

        var createEventsTable = @"
            CREATE TABLE IF NOT EXISTS Events (
                Id TEXT PRIMARY KEY,
                CalendarId TEXT NOT NULL,
                Title TEXT,
                StartTime INTEGER NOT NULL,
                EndTime INTEGER NOT NULL,
                Description TEXT,
                Location TEXT,
                IsBlockingEvent INTEGER NOT NULL,
                SourceEventId TEXT,
                FOREIGN KEY (CalendarId) REFERENCES Calendars(Id)
            )";

        await using var cmd1 = new SqliteCommand(createCalendarsTable, connection);
        await cmd1.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        await using var cmd2 = new SqliteCommand(createEventsTable, connection);
        await cmd2.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
    
    public async Task DeleteCalendarAsync(string calendarId, CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var commandEvents = new SqliteCommand("DELETE FROM Events WHERE CalendarId = @CalendarId", connection);
        commandEvents.Parameters.AddWithValue("@CalendarId", calendarId);
        await commandEvents.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        await using var command = new SqliteCommand("DELETE FROM Calendars WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", calendarId);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    public async Task<List<Calendar>> GetAllCalendarsAsync(CancellationToken ct = default)
    {
        var calendars = new List<Calendar>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var sql = "SELECT Id, Name, Color, Type, EnableBlocking, ReceiveBlocks FROM Calendars";

        await using var command = new SqliteCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);

        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            var calendar = new Calendar
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Color = reader.GetString(2),
                Type = (CalendarType)reader.GetInt32(3),
                EnableBlocking = reader.GetBoolean(4),
                ReceiveBlocks = reader.GetBoolean(5)
            };
            calendars.Add(calendar);
        }

        return calendars;
    }

    public async Task CreateCalendarAsync(Calendar calendar, CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        var sql = @"
            INSERT INTO Calendars (Id, Name, Color, Type, EnableBlocking, ReceiveBlocks)
            VALUES (@Id, @Name, @Color, @Type, @EnableBlocking, @ReceiveBlocks)";
        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", calendar.Id);
        command.Parameters.AddWithValue("@Name", calendar.Name);
        command.Parameters.AddWithValue("@Color", calendar.Color);
        command.Parameters.AddWithValue("@Type", (int)calendar.Type);
        command.Parameters.AddWithValue("@EnableBlocking", calendar.EnableBlocking);
        command.Parameters.AddWithValue("@ReceiveBlocks", calendar.ReceiveBlocks);
        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    public async Task ResetDatabaseAsync(CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var dropEvents = new SqliteCommand("DROP TABLE IF EXISTS Events", connection);
        await dropEvents.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        await using var dropCalendars = new SqliteCommand("DROP TABLE IF EXISTS Calendars", connection);
        await dropCalendars.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        await connection.CloseAsync().ConfigureAwait(false);
        await InitializeDatabaseAsync(ct).ConfigureAwait(false);
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

    public async Task<Dictionary<string, Calendar>> GetCalendarLookupAsync(CancellationToken ct = default)
    {
        var calendars = new Dictionary<string, Calendar>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var sql = "SELECT Id, Name, Color, Type, EnableBlocking, ReceiveBlocks FROM Calendars";

        await using var command = new SqliteCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);

        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            var calendar = new Calendar
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Color = reader.GetString(2),
                Type = (CalendarType)reader.GetInt32(3),
                EnableBlocking = reader.GetBoolean(4),
                ReceiveBlocks = reader.GetBoolean(5)
            };
            calendars[calendar.Id] = calendar;
        }

        return calendars;
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

