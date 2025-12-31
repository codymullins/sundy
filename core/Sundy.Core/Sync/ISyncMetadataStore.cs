namespace Sundy.Core.Sync;

/// <summary>
/// Store for sync-related metadata like device ID, server URL, and sync state.
/// </summary>
public interface ISyncMetadataStore
{
    /// <summary>
    /// Get the device ID for this client.
    /// </summary>
    Task<string?> GetDeviceIdAsync(CancellationToken ct = default);

    /// <summary>
    /// Set the device ID for this client.
    /// </summary>
    Task SetDeviceIdAsync(string deviceId, CancellationToken ct = default);

    /// <summary>
    /// Get the device token for authenticating with the server.
    /// </summary>
    Task<string?> GetDeviceTokenAsync(CancellationToken ct = default);

    /// <summary>
    /// Set the device token.
    /// </summary>
    Task SetDeviceTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Get the last known server version.
    /// </summary>
    Task<long> GetLastServerVersionAsync(CancellationToken ct = default);

    /// <summary>
    /// Set the last known server version.
    /// </summary>
    Task SetLastServerVersionAsync(long version, CancellationToken ct = default);

    /// <summary>
    /// Get the configured server URL.
    /// </summary>
    Task<string?> GetServerUrlAsync(CancellationToken ct = default);

    /// <summary>
    /// Set the server URL.
    /// </summary>
    Task SetServerUrlAsync(string url, CancellationToken ct = default);

    /// <summary>
    /// Check if sync is enabled.
    /// </summary>
    Task<bool> IsSyncEnabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Enable or disable sync.
    /// </summary>
    Task SetSyncEnabledAsync(bool enabled, CancellationToken ct = default);

    /// <summary>
    /// Get the last sync timestamp.
    /// </summary>
    Task<DateTimeOffset?> GetLastSyncAtAsync(CancellationToken ct = default);

    /// <summary>
    /// Set the last sync timestamp.
    /// </summary>
    Task SetLastSyncAtAsync(DateTimeOffset timestamp, CancellationToken ct = default);

    /// <summary>
    /// Clear all sync metadata.
    /// </summary>
    Task ClearAllAsync(CancellationToken ct = default);
}
