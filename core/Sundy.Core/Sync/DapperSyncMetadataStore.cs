using System.Data;
using Dapper;

namespace Sundy.Core.Sync;

/// <summary>
/// Dapper implementation of ISyncMetadataStore using SQLite.
/// </summary>
public class DapperSyncMetadataStore(IDbConnection connection) : ISyncMetadataStore
{
    private const string KeyDeviceId = "DeviceId";
    private const string KeyDeviceToken = "DeviceToken";
    private const string KeyLastServerVersion = "LastServerVersion";
    private const string KeyServerUrl = "ServerUrl";
    private const string KeySyncEnabled = "SyncEnabled";
    private const string KeyLastSyncAt = "LastSyncAt";

    public async Task<string?> GetDeviceIdAsync(CancellationToken ct = default)
        => await GetValueAsync(KeyDeviceId, ct).ConfigureAwait(false);

    public async Task SetDeviceIdAsync(string deviceId, CancellationToken ct = default)
        => await SetValueAsync(KeyDeviceId, deviceId, ct).ConfigureAwait(false);

    public async Task<string?> GetDeviceTokenAsync(CancellationToken ct = default)
        => await GetValueAsync(KeyDeviceToken, ct).ConfigureAwait(false);

    public async Task SetDeviceTokenAsync(string token, CancellationToken ct = default)
        => await SetValueAsync(KeyDeviceToken, token, ct).ConfigureAwait(false);

    public async Task<long> GetLastServerVersionAsync(CancellationToken ct = default)
    {
        var value = await GetValueAsync(KeyLastServerVersion, ct).ConfigureAwait(false);
        return long.TryParse(value, out var version) ? version : 0;
    }

    public async Task SetLastServerVersionAsync(long version, CancellationToken ct = default)
        => await SetValueAsync(KeyLastServerVersion, version.ToString(), ct).ConfigureAwait(false);

    public async Task<string?> GetServerUrlAsync(CancellationToken ct = default)
        => await GetValueAsync(KeyServerUrl, ct).ConfigureAwait(false);

    public async Task SetServerUrlAsync(string url, CancellationToken ct = default)
        => await SetValueAsync(KeyServerUrl, url, ct).ConfigureAwait(false);

    public async Task<bool> IsSyncEnabledAsync(CancellationToken ct = default)
    {
        var value = await GetValueAsync(KeySyncEnabled, ct).ConfigureAwait(false);
        return value == "true";
    }

    public async Task SetSyncEnabledAsync(bool enabled, CancellationToken ct = default)
        => await SetValueAsync(KeySyncEnabled, enabled ? "true" : "false", ct).ConfigureAwait(false);

    public async Task<DateTimeOffset?> GetLastSyncAtAsync(CancellationToken ct = default)
    {
        var value = await GetValueAsync(KeyLastSyncAt, ct).ConfigureAwait(false);
        return value != null && DateTimeOffset.TryParse(value, out var timestamp) ? timestamp : null;
    }

    public async Task SetLastSyncAtAsync(DateTimeOffset timestamp, CancellationToken ct = default)
        => await SetValueAsync(KeyLastSyncAt, timestamp.ToString("o"), ct).ConfigureAwait(false);

    public async Task ClearAllAsync(CancellationToken ct = default)
    {
        const string sql = "DELETE FROM SyncMetadata";
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
    }

    private async Task<string?> GetValueAsync(string key, CancellationToken ct)
    {
        const string sql = "SELECT Value FROM SyncMetadata WHERE Key = @Key";
        return await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { Key = key }, cancellationToken: ct)).ConfigureAwait(false);
    }

    private async Task SetValueAsync(string key, string value, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO SyncMetadata (Key, Value) VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value
            """;
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Key = key, Value = value }, cancellationToken: ct)).ConfigureAwait(false);
    }
}
