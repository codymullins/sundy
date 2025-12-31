using System.Text.Json;

namespace Sundy.Core.Sync;

/// <summary>
/// Records operations (insert/update/delete) for sync when enabled.
/// Injected into stores to capture local changes.
/// </summary>
public class OperationRecorder(
    ISyncMetadataStore metadataStore,
    IUploadQueueStore uploadQueueStore)
{
    /// <summary>
    /// Record an insert operation for sync.
    /// </summary>
    public async Task RecordInsertAsync<T>(EntityType entityType, string entityId, T entity, CancellationToken ct = default)
    {
        if (!await metadataStore.IsSyncEnabledAsync(ct).ConfigureAwait(false))
            return;

        var payload = JsonSerializer.Serialize(entity);
        var op = await CreateOperationAsync(entityType, OperationType.Insert, entityId, payload, ct).ConfigureAwait(false);
        await uploadQueueStore.EnqueueAsync(op, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Record an update operation for sync.
    /// </summary>
    public async Task RecordUpdateAsync<T>(EntityType entityType, string entityId, T entity, CancellationToken ct = default)
    {
        if (!await metadataStore.IsSyncEnabledAsync(ct).ConfigureAwait(false))
            return;

        var payload = JsonSerializer.Serialize(entity);
        var op = await CreateOperationAsync(entityType, OperationType.Update, entityId, payload, ct).ConfigureAwait(false);
        await uploadQueueStore.EnqueueAsync(op, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Record a delete operation for sync.
    /// </summary>
    public async Task RecordDeleteAsync(EntityType entityType, string entityId, CancellationToken ct = default)
    {
        if (!await metadataStore.IsSyncEnabledAsync(ct).ConfigureAwait(false))
            return;

        var op = await CreateOperationAsync(entityType, OperationType.Delete, entityId, null, ct).ConfigureAwait(false);
        await uploadQueueStore.EnqueueAsync(op, ct).ConfigureAwait(false);
    }

    private async Task<Operation> CreateOperationAsync(EntityType entityType, OperationType opType, string entityId, string? payload, CancellationToken ct)
    {
        var deviceId = await metadataStore.GetDeviceIdAsync(ct).ConfigureAwait(false) ?? "unknown";
        var clientVersion = await metadataStore.GetLastServerVersionAsync(ct).ConfigureAwait(false);

        return new Operation
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = deviceId,
            EntityType = entityType,
            OpType = opType,
            EntityId = entityId,
            Payload = payload,
            ClientVersion = clientVersion,
            Timestamp = DateTimeOffset.UtcNow,
            Attempts = 0
        };
    }
}
