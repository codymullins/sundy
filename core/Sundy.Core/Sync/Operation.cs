namespace Sundy.Core.Sync;

/// <summary>
/// Types of operations that can be performed on entities.
/// </summary>
public enum OperationType
{
    Insert = 0,
    Update = 1,
    Delete = 2
}

/// <summary>
/// Types of entities that can be synced.
/// </summary>
public enum EntityType
{
    Calendar = 0,
    Event = 1,
    Setting = 2
}

/// <summary>
/// Represents a single operation/changeset for sync.
/// Operations are recorded locally and uploaded to the server.
/// </summary>
public record Operation
{
    /// <summary>
    /// Unique identifier for this operation (GUID).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The device that created this operation.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// The type of entity being modified.
    /// </summary>
    public required EntityType EntityType { get; init; }

    /// <summary>
    /// The type of operation (insert/update/delete).
    /// </summary>
    public required OperationType OpType { get; init; }

    /// <summary>
    /// The ID of the entity being modified.
    /// </summary>
    public required string EntityId { get; init; }

    /// <summary>
    /// JSON payload containing the entity data or changed fields.
    /// For INSERT: full entity JSON.
    /// For UPDATE: JSON with only changed fields.
    /// For DELETE: null.
    /// </summary>
    public string? Payload { get; init; }

    /// <summary>
    /// The server version the client was at when this operation was created.
    /// Used for operational transformation on the server.
    /// </summary>
    public long ClientVersion { get; init; }

    /// <summary>
    /// When this operation was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// The server version assigned after the operation was applied.
    /// Null until the server processes the operation.
    /// </summary>
    public long? ServerVersion { get; init; }

    /// <summary>
    /// Number of upload attempts (for retry logic).
    /// </summary>
    public int Attempts { get; init; }
}
