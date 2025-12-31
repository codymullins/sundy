namespace Sundy.Core.Sync;

/// <summary>
/// Store for managing the upload queue of pending operations.
/// Operations are queued locally and uploaded to the server when online.
/// </summary>
public interface IUploadQueueStore
{
    /// <summary>
    /// Add an operation to the upload queue.
    /// </summary>
    Task EnqueueAsync(Operation op, CancellationToken ct = default);

    /// <summary>
    /// Get pending operations from the queue, ordered by timestamp.
    /// </summary>
    Task<List<Operation>> GetPendingAsync(int limit = 100, CancellationToken ct = default);

    /// <summary>
    /// Mark an operation as successfully uploaded and remove it from the queue.
    /// </summary>
    Task RemoveAsync(string opId, CancellationToken ct = default);

    /// <summary>
    /// Increment the attempt count for a failed upload.
    /// </summary>
    Task IncrementAttemptsAsync(string opId, CancellationToken ct = default);

    /// <summary>
    /// Get the count of pending operations in the queue.
    /// </summary>
    Task<int> GetPendingCountAsync(CancellationToken ct = default);

    /// <summary>
    /// Clear all operations from the queue.
    /// </summary>
    Task ClearAsync(CancellationToken ct = default);
}
