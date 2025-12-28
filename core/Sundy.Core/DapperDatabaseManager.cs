using System.Data;
using Dapper;

namespace Sundy.Core;

public class DapperDatabaseManager(IDbConnection connection)
{
    private const string CreateCalendarsTableSql = """
        CREATE TABLE IF NOT EXISTS Calendars (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            Color TEXT NOT NULL,
            Type INTEGER NOT NULL,
            EnableBlocking INTEGER NOT NULL,
            ReceiveBlocks INTEGER NOT NULL
        )
        """;

    private const string CreateEventsTableSql = """
        CREATE TABLE IF NOT EXISTS Events (
            Id TEXT PRIMARY KEY NOT NULL,
            CalendarId TEXT NOT NULL,
            Title TEXT,
            StartTime TEXT NOT NULL,
            EndTime TEXT NOT NULL,
            Description TEXT,
            Location TEXT,
            IsBlockingEvent INTEGER NOT NULL,
            SourceEventId TEXT,
            FOREIGN KEY (CalendarId) REFERENCES Calendars(Id) ON DELETE CASCADE
        )
        """;

    private const string DropCalendarsTableSql = "DROP TABLE IF EXISTS Calendars";
    private const string DropEventsTableSql = "DROP TABLE IF EXISTS Events";

    public async Task<bool> DatabaseExistsAsync(CancellationToken ct = default)
    {
        try
        {
            // Check if we can query the Calendars table
            const string sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Calendars'";
            var command = new CommandDefinition(sql, cancellationToken: ct);
            var count = await connection.ExecuteScalarAsync<int>(command).ConfigureAwait(false);
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task InitializeDatabaseAsync(CancellationToken ct = default)
    {
        // Enable foreign keys
        await connection.ExecuteAsync(new CommandDefinition(
            "PRAGMA foreign_keys = ON", cancellationToken: ct)).ConfigureAwait(false);

        // Create tables
        await connection.ExecuteAsync(new CommandDefinition(
            CreateCalendarsTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            CreateEventsTableSql, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task DeleteDatabaseAsync(CancellationToken ct = default)
    {
        // Drop tables in reverse order due to foreign key constraints
        await connection.ExecuteAsync(new CommandDefinition(
            DropEventsTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            DropCalendarsTableSql, cancellationToken: ct)).ConfigureAwait(false);
    }
}
