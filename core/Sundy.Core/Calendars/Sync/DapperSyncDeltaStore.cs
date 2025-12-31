using System.Data;
using Dapper;

namespace Sundy.Core.Calendars.Sync;

/// <summary>
/// Dapper implementation of ISyncDeltaStore using SQLite.
/// Stores Microsoft Graph delta tokens for incremental calendar sync.
/// </summary>
public class DapperSyncDeltaStore(IDbConnection connection) : ISyncDeltaStore
{
    private const string CreateTableSql = """
        CREATE TABLE IF NOT EXISTS SyncDeltaTokens (
            CalendarId TEXT PRIMARY KEY NOT NULL,
            DeltaToken TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        )
        """;

    /// <summary>
    /// Ensures the SyncDeltaTokens table exists.
    /// Call during app initialization.
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await connection.ExecuteAsync(
            new CommandDefinition(CreateTableSql, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<string?> GetDeltaTokenAsync(string calendarId, CancellationToken ct = default)
    {
        const string sql = "SELECT DeltaToken FROM SyncDeltaTokens WHERE CalendarId = @CalendarId";
        return await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { CalendarId = calendarId }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task SaveDeltaTokenAsync(string calendarId, string deltaToken, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO SyncDeltaTokens (CalendarId, DeltaToken, UpdatedAt)
            VALUES (@CalendarId, @DeltaToken, @UpdatedAt)
            ON CONFLICT(CalendarId) DO UPDATE SET
                DeltaToken = @DeltaToken,
                UpdatedAt = @UpdatedAt
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                CalendarId = calendarId,
                DeltaToken = deltaToken,
                UpdatedAt = DateTime.UtcNow.ToString("o")
            }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task ClearDeltaTokenAsync(string calendarId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM SyncDeltaTokens WHERE CalendarId = @CalendarId";
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { CalendarId = calendarId }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task ClearAllAsync(CancellationToken ct = default)
    {
        const string sql = "DELETE FROM SyncDeltaTokens";
        await connection.ExecuteAsync(
            new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
    }
}
