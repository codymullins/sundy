using Microsoft.Data.Sqlite;

namespace Sundy.Core;

public class CalendarStore(string connectionString)
{
    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
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
}