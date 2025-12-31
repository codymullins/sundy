using System.Data;
using Dapper;

namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Dapper-based implementation of IConnectedAccountStore using SQLite.
/// </summary>
public class DapperConnectedAccountStore(IDbConnection connection) : IConnectedAccountStore
{
    public async Task<List<ConnectedAccount>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Email, DisplayName, ProviderType, ConnectedAt, LastSyncAt, Status,
                   AccessToken, RefreshToken, TokenExpiresAt
            FROM ConnectedAccounts
            """;
        var command = new CommandDefinition(sql, cancellationToken: ct);
        var results = await connection.QueryAsync<ConnectedAccountDto>(command).ConfigureAwait(false);
        return results.Select(MapFromDto).ToList();
    }

    public async Task<ConnectedAccount?> GetByIdAsync(string accountId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Email, DisplayName, ProviderType, ConnectedAt, LastSyncAt, Status,
                   AccessToken, RefreshToken, TokenExpiresAt
            FROM ConnectedAccounts
            WHERE Id = @Id
            """;
        var command = new CommandDefinition(sql, new { Id = accountId }, cancellationToken: ct);
        var result = await connection.QueryFirstOrDefaultAsync<ConnectedAccountDto>(command).ConfigureAwait(false);
        return result is null ? null : MapFromDto(result);
    }

    public async Task AddAsync(ConnectedAccount account, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO ConnectedAccounts (Id, Email, DisplayName, ProviderType, ConnectedAt, LastSyncAt, Status,
                                           AccessToken, RefreshToken, TokenExpiresAt)
            VALUES (@Id, @Email, @DisplayName, @ProviderType, @ConnectedAt, @LastSyncAt, @Status,
                    @AccessToken, @RefreshToken, @TokenExpiresAt)
            """;
        var command = new CommandDefinition(sql, new
        {
            account.Id,
            account.Email,
            account.DisplayName,
            ProviderType = (int)account.ProviderType,
            ConnectedAt = account.ConnectedAt.ToString("o"),
            LastSyncAt = account.LastSyncAt?.ToString("o"),
            Status = (int)account.Status,
            account.AccessToken,
            account.RefreshToken,
            TokenExpiresAt = account.TokenExpiresAt?.ToString("o")
        }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public async Task UpdateAsync(ConnectedAccount account, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE ConnectedAccounts
            SET Email = @Email,
                DisplayName = @DisplayName,
                ProviderType = @ProviderType,
                ConnectedAt = @ConnectedAt,
                LastSyncAt = @LastSyncAt,
                Status = @Status,
                AccessToken = @AccessToken,
                RefreshToken = @RefreshToken,
                TokenExpiresAt = @TokenExpiresAt
            WHERE Id = @Id
            """;
        var command = new CommandDefinition(sql, new
        {
            account.Id,
            account.Email,
            account.DisplayName,
            ProviderType = (int)account.ProviderType,
            ConnectedAt = account.ConnectedAt.ToString("o"),
            LastSyncAt = account.LastSyncAt?.ToString("o"),
            Status = (int)account.Status,
            account.AccessToken,
            account.RefreshToken,
            TokenExpiresAt = account.TokenExpiresAt?.ToString("o")
        }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string accountId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM ConnectedAccounts WHERE Id = @Id";
        var command = new CommandDefinition(sql, new { Id = accountId }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public async Task<List<ConnectedAccount>> GetByProviderTypeAsync(ProviderType providerType, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Email, DisplayName, ProviderType, ConnectedAt, LastSyncAt, Status,
                   AccessToken, RefreshToken, TokenExpiresAt
            FROM ConnectedAccounts
            WHERE ProviderType = @ProviderType
            """;
        var command = new CommandDefinition(sql, new { ProviderType = (int)providerType }, cancellationToken: ct);
        var results = await connection.QueryAsync<ConnectedAccountDto>(command).ConfigureAwait(false);
        return results.Select(MapFromDto).ToList();
    }

    private static ConnectedAccount MapFromDto(ConnectedAccountDto dto) => new()
    {
        Id = dto.Id,
        Email = dto.Email,
        DisplayName = dto.DisplayName,
        ProviderType = (ProviderType)dto.ProviderType,
        ConnectedAt = ParseDateTimeOffset(dto.ConnectedAt) ?? DateTimeOffset.MinValue,
        LastSyncAt = ParseDateTimeOffset(dto.LastSyncAt),
        Status = (AccountStatus)dto.Status,
        AccessToken = dto.AccessToken,
        RefreshToken = dto.RefreshToken,
        TokenExpiresAt = ParseDateTimeOffset(dto.TokenExpiresAt)
    };

    private static DateTimeOffset? ParseDateTimeOffset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, out var result))
        {
            return result;
        }

        if (DateTime.TryParse(value, out var dateTime))
        {
            return new DateTimeOffset(dateTime);
        }

        return null;
    }

    // DTO for Dapper mapping since SQLite stores enums as int and DateTimeOffset as string
    private sealed class ConnectedAccountDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int ProviderType { get; set; }
        public string ConnectedAt { get; set; } = string.Empty;
        public string? LastSyncAt { get; set; }
        public int Status { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? TokenExpiresAt { get; set; }
    }
}
