using System.Data;
using Dapper;

namespace Sundy.Core;

public class DapperDatabaseManager(IDbConnection connection)
{
    private const string CreateCalendarsTableSql = """
        CREATE TABLE IF NOT EXISTS Calendars (
            Id TEXT PRIMARY KEY NOT NULL,
            Name TEXT NOT NULL,
            DisplayName TEXT,
            Color TEXT NOT NULL,
            Type INTEGER NOT NULL,
            EnableBlocking INTEGER NOT NULL,
            ReceiveBlocks INTEGER NOT NULL,
            ExternalAccountId TEXT,
            IsHidden INTEGER NOT NULL DEFAULT 0,
            Version INTEGER NOT NULL DEFAULT 0,
            UpdatedAt TEXT NOT NULL DEFAULT '',
            IsDeleted INTEGER NOT NULL DEFAULT 0
        )
        """;

    private const string CreateConnectedAccountsTableSql = """
        CREATE TABLE IF NOT EXISTS ConnectedAccounts (
            Id TEXT PRIMARY KEY NOT NULL,
            Email TEXT NOT NULL,
            DisplayName TEXT NOT NULL,
            ProviderType INTEGER NOT NULL,
            ConnectedAt TEXT NOT NULL,
            LastSyncAt TEXT,
            Status INTEGER NOT NULL,
            AccessToken TEXT,
            RefreshToken TEXT,
            TokenExpiresAt TEXT
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
            Version INTEGER NOT NULL DEFAULT 0,
            UpdatedAt TEXT NOT NULL DEFAULT '',
            IsDeleted INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (CalendarId) REFERENCES Calendars(Id) ON DELETE CASCADE
        )
        """;

    private const string CreateSettingsTableSql = """
        CREATE TABLE IF NOT EXISTS Settings (
            Key TEXT PRIMARY KEY NOT NULL,
            Value TEXT NOT NULL,
            Version INTEGER NOT NULL DEFAULT 0,
            UpdatedAt TEXT NOT NULL DEFAULT '',
            IsDeleted INTEGER NOT NULL DEFAULT 0
        )
        """;

    private const string CreateUploadQueueTableSql = """
        CREATE TABLE IF NOT EXISTS UploadQueue (
            Id TEXT PRIMARY KEY NOT NULL,
            DeviceId TEXT NOT NULL,
            EntityType INTEGER NOT NULL,
            OpType INTEGER NOT NULL,
            EntityId TEXT NOT NULL,
            Payload TEXT,
            ClientVersion INTEGER NOT NULL,
            Timestamp TEXT NOT NULL,
            Attempts INTEGER NOT NULL DEFAULT 0
        )
        """;

    private const string CreateSyncMetadataTableSql = """
        CREATE TABLE IF NOT EXISTS SyncMetadata (
            Key TEXT PRIMARY KEY NOT NULL,
            Value TEXT NOT NULL
        )
        """;

    private const string CreateSyncDeltaTokensTableSql = """
        CREATE TABLE IF NOT EXISTS SyncDeltaTokens (
            CalendarId TEXT PRIMARY KEY NOT NULL,
            DeltaToken TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        )
        """;

    private const string DropCalendarsTableSql = "DROP TABLE IF EXISTS Calendars";
    private const string DropEventsTableSql = "DROP TABLE IF EXISTS Events";
    private const string DropConnectedAccountsTableSql = "DROP TABLE IF EXISTS ConnectedAccounts";
    private const string DropSettingsTableSql = "DROP TABLE IF EXISTS Settings";
    private const string DropUploadQueueTableSql = "DROP TABLE IF EXISTS UploadQueue";
    private const string DropSyncMetadataTableSql = "DROP TABLE IF EXISTS SyncMetadata";
    private const string DropSyncDeltaTokensTableSql = "DROP TABLE IF EXISTS SyncDeltaTokens";

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
        await connection.ExecuteAsync(new CommandDefinition(
            CreateConnectedAccountsTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            CreateSettingsTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            CreateUploadQueueTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            CreateSyncMetadataTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            CreateSyncDeltaTokensTableSql, cancellationToken: ct)).ConfigureAwait(false);

        // Run migrations for existing databases
        await MigrateSchemaAsync(ct).ConfigureAwait(false);
    }

    private async Task MigrateSchemaAsync(CancellationToken ct = default)
    {
        // Migration: Add ExternalAccountId to Calendars
        await AddColumnIfNotExistsAsync("Calendars", "ExternalAccountId", "TEXT", ct).ConfigureAwait(false);

        // Migration: Add sync columns to Calendars
        await AddColumnIfNotExistsAsync("Calendars", "Version", "INTEGER NOT NULL DEFAULT 0", ct).ConfigureAwait(false);
        await AddColumnIfNotExistsAsync("Calendars", "UpdatedAt", "TEXT NOT NULL DEFAULT ''", ct).ConfigureAwait(false);
        await AddColumnIfNotExistsAsync("Calendars", "IsDeleted", "INTEGER NOT NULL DEFAULT 0", ct).ConfigureAwait(false);

        // Migration: Add sync columns to Events
        await AddColumnIfNotExistsAsync("Events", "Version", "INTEGER NOT NULL DEFAULT 0", ct).ConfigureAwait(false);
        await AddColumnIfNotExistsAsync("Events", "UpdatedAt", "TEXT NOT NULL DEFAULT ''", ct).ConfigureAwait(false);
        await AddColumnIfNotExistsAsync("Events", "IsDeleted", "INTEGER NOT NULL DEFAULT 0", ct).ConfigureAwait(false);

        // Migration: Add token columns to ConnectedAccounts (for WASM HTTP-based auth)
        await AddColumnIfNotExistsAsync("ConnectedAccounts", "AccessToken", "TEXT", ct).ConfigureAwait(false);
        await AddColumnIfNotExistsAsync("ConnectedAccounts", "RefreshToken", "TEXT", ct).ConfigureAwait(false);
        await AddColumnIfNotExistsAsync("ConnectedAccounts", "TokenExpiresAt", "TEXT", ct).ConfigureAwait(false);

        // Migration: Add IsHidden column to Calendars (for hiding system calendars)
        await AddColumnIfNotExistsAsync("Calendars", "IsHidden", "INTEGER NOT NULL DEFAULT 0", ct).ConfigureAwait(false);

        // Migration: Add DisplayName column to Calendars (for custom calendar names)
        await AddColumnIfNotExistsAsync("Calendars", "DisplayName", "TEXT", ct).ConfigureAwait(false);

        // Migration: Add external sync columns to Events (for Outlook sync)
        await AddColumnIfNotExistsAsync("Events", "ExternalId", "TEXT", ct).ConfigureAwait(false);
        await AddColumnIfNotExistsAsync("Events", "ExternalModifiedAt", "TEXT", ct).ConfigureAwait(false);

        // Backfill UpdatedAt for existing records
        await BackfillUpdatedAtAsync(ct).ConfigureAwait(false);
    }

    private async Task AddColumnIfNotExistsAsync(string table, string column, string definition, CancellationToken ct)
    {
        var checkColumnSql = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='{column}'";
        var columnExists = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(checkColumnSql, cancellationToken: ct)).ConfigureAwait(false);

        if (columnExists == 0)
        {
            var addColumnSql = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
            await connection.ExecuteAsync(new CommandDefinition(addColumnSql, cancellationToken: ct)).ConfigureAwait(false);
        }
    }

    private async Task BackfillUpdatedAtAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow.ToString("o");

        // Backfill Calendars
        await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE Calendars SET UpdatedAt = @Now WHERE UpdatedAt = ''",
            new { Now = now }, cancellationToken: ct)).ConfigureAwait(false);

        // Backfill Events
        await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE Events SET UpdatedAt = @Now WHERE UpdatedAt = ''",
            new { Now = now }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task DeleteDatabaseAsync(CancellationToken ct = default)
    {
        // Drop tables in reverse order due to foreign key constraints
        await connection.ExecuteAsync(new CommandDefinition(
            DropEventsTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            DropCalendarsTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            DropConnectedAccountsTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            DropSettingsTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            DropUploadQueueTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            DropSyncMetadataTableSql, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            DropSyncDeltaTokensTableSql, cancellationToken: ct)).ConfigureAwait(false);
    }
}
