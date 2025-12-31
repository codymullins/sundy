using System.Data;
using Dapper;

namespace Sundy.Core.Sync;

/// <summary>
/// Dapper implementation of IUploadQueueStore using SQLite.
/// </summary>
public class DapperUploadQueueStore(IDbConnection connection) : IUploadQueueStore
{
    public async Task EnqueueAsync(Operation op, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO UploadQueue (Id, DeviceId, EntityType, OpType, EntityId, Payload, ClientVersion, Timestamp, Attempts)
            VALUES (@Id, @DeviceId, @EntityType, @OpType, @EntityId, @Payload, @ClientVersion, @Timestamp, @Attempts)
            """;

        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            op.Id,
            op.DeviceId,
            EntityType = (int)op.EntityType,
            OpType = (int)op.OpType,
            op.EntityId,
            op.Payload,
            op.ClientVersion,
            Timestamp = op.Timestamp.ToString("o"),
            op.Attempts
        }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<List<Operation>> GetPendingAsync(int limit = 100, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, DeviceId, EntityType, OpType, EntityId, Payload, ClientVersion, Timestamp, Attempts
            FROM UploadQueue
            ORDER BY Timestamp ASC
            LIMIT @Limit
            """;

        var rows = await connection.QueryAsync<OperationDto>(
            new CommandDefinition(sql, new { Limit = limit }, cancellationToken: ct)).ConfigureAwait(false);

        return rows.Select(r => new Operation
        {
            Id = r.Id,
            DeviceId = r.DeviceId,
            EntityType = (EntityType)r.EntityType,
            OpType = (OperationType)r.OpType,
            EntityId = r.EntityId,
            Payload = r.Payload,
            ClientVersion = r.ClientVersion,
            Timestamp = DateTimeOffset.Parse(r.Timestamp),
            Attempts = r.Attempts
        }).ToList();
    }

    public async Task RemoveAsync(string opId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM UploadQueue WHERE Id = @Id";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = opId }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task IncrementAttemptsAsync(string opId, CancellationToken ct = default)
    {
        const string sql = "UPDATE UploadQueue SET Attempts = Attempts + 1 WHERE Id = @Id";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = opId }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<int> GetPendingCountAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(*) FROM UploadQueue";
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        const string sql = "DELETE FROM UploadQueue";
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
    }

    private sealed class OperationDto
    {
        public string Id { get; set; } = "";
        public string DeviceId { get; set; } = "";
        public int EntityType { get; set; }
        public int OpType { get; set; }
        public string EntityId { get; set; } = "";
        public string? Payload { get; set; }
        public long ClientVersion { get; set; }
        public string Timestamp { get; set; } = "";
        public int Attempts { get; set; }
    }
}
