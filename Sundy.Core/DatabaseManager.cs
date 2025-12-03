using Microsoft.Data.Sqlite;

namespace Sundy.Core;

public class DatabaseManager(string connectionString)
{
    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }
    
    public async Task <bool> DatabaseExistsAsync(CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var checkCalendarsTable = @"
            SELECT name
            FROM sqlite_master
            WHERE type='table' AND name='Calendars'";

        await using var cmd = new SqliteCommand(checkCalendarsTable, connection);
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);

        return result != null && result != DBNull.Value;
    }
    
    public static async Task CreateDatabaseFileAsync(string connectionString, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        
        // make sure we create the file
        var manager = new DatabaseManager(connectionString);
        await using var connection = manager.CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        await connection.CloseAsync().ConfigureAwait(false);
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
    
    public async Task DeleteDatabaseAsync(CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var dropEvents = new SqliteCommand("DROP TABLE IF EXISTS Events", connection);
        await dropEvents.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        await using var dropCalendars = new SqliteCommand("DROP TABLE IF EXISTS Calendars", connection);
        await dropCalendars.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        await connection.CloseAsync().ConfigureAwait(false);
    }
}