namespace Sundy.Core.Sync;

/// <summary>
/// Client-side sync service for managing synchronization with the server.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// The device ID assigned by the server.
    /// </summary>
    string? DeviceId { get; }

    /// <summary>
    /// Whether sync is enabled and configured.
    /// </summary>
    bool IsSyncEnabled { get; }

    /// <summary>
    /// Whether the client is currently online and able to sync.
    /// </summary>
    bool IsOnline { get; }

    /// <summary>
    /// Number of operations pending upload.
    /// </summary>
    int PendingUploadCount { get; }

    /// <summary>
    /// Last known server version.
    /// </summary>
    long LastServerVersion { get; }

    /// <summary>
    /// Last successful sync timestamp.
    /// </summary>
    DateTimeOffset? LastSyncAt { get; }

    /// <summary>
    /// Event raised when sync status changes.
    /// </summary>
    event EventHandler<SyncStatusChangedEventArgs>? StatusChanged;

    /// <summary>
    /// Register this device with the server.
    /// </summary>
    Task<bool> RegisterDeviceAsync(string serverUrl, string? deviceName = null, CancellationToken ct = default);

    /// <summary>
    /// Perform a full sync cycle (download then upload).
    /// </summary>
    Task<SyncResult> SyncAsync(CancellationToken ct = default);

    /// <summary>
    /// Disable sync and clear all sync metadata.
    /// </summary>
    Task DisableSyncAsync(CancellationToken ct = default);

    /// <summary>
    /// Initialize the sync service (load state from metadata store).
    /// </summary>
    Task InitializeAsync(CancellationToken ct = default);
}

/// <summary>
/// Result of a sync operation.
/// </summary>
public record SyncResult(
    bool Success,
    int Downloaded,
    int Uploaded,
    long ServerVersion,
    string? Error);

/// <summary>
/// Event args for sync status changes.
/// </summary>
public class SyncStatusChangedEventArgs : EventArgs
{
    public bool IsOnline { get; init; }
    public bool IsSyncing { get; init; }
    public int PendingUploadCount { get; init; }
    public string? Message { get; init; }
}
