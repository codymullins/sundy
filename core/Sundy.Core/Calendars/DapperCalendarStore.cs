using System.Data;
using Dapper;

namespace Sundy.Core;

public class DapperCalendarStore(IDbConnection connection) : ICalendarStore
{
    public async Task DeleteCalendarAsync(string calendarId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM Calendars WHERE Id = @Id";
        var command = new CommandDefinition(sql, new { Id = calendarId }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public async Task<List<Calendar>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT Id, Name, DisplayName, Color, Type, EnableBlocking, ReceiveBlocks, ExternalAccountId, IsHidden FROM Calendars";
        var command = new CommandDefinition(sql, cancellationToken: ct);
        var results = await connection.QueryAsync<CalendarDto>(command).ConfigureAwait(false);
        return results.Select(MapFromDto).ToList();
    }

    public async Task CreateCalendarAsync(Calendar calendar, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Calendars (Id, Name, DisplayName, Color, Type, EnableBlocking, ReceiveBlocks, ExternalAccountId, IsHidden)
            VALUES (@Id, @Name, @DisplayName, @Color, @Type, @EnableBlocking, @ReceiveBlocks, @ExternalAccountId, @IsHidden)
            """;
        var command = new CommandDefinition(sql, new
        {
            calendar.Id,
            calendar.Name,
            calendar.DisplayName,
            calendar.Color,
            Type = (int)calendar.Type,
            EnableBlocking = calendar.EnableBlocking ? 1 : 0,
            ReceiveBlocks = calendar.ReceiveBlocks ? 1 : 0,
            calendar.ExternalAccountId,
            IsHidden = calendar.IsHidden ? 1 : 0
        }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public async Task UpdateCalendarAsync(Calendar calendar, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Calendars
            SET Name = @Name,
                DisplayName = @DisplayName,
                Color = @Color,
                EnableBlocking = @EnableBlocking,
                ReceiveBlocks = @ReceiveBlocks,
                IsHidden = @IsHidden,
                Version = Version + 1,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        var command = new CommandDefinition(sql, new
        {
            calendar.Id,
            calendar.Name,
            calendar.DisplayName,
            calendar.Color,
            EnableBlocking = calendar.EnableBlocking ? 1 : 0,
            ReceiveBlocks = calendar.ReceiveBlocks ? 1 : 0,
            IsHidden = calendar.IsHidden ? 1 : 0,
            UpdatedAt = DateTimeOffset.UtcNow.ToString("o")
        }, cancellationToken: ct);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public Task AddAsync(Calendar calendar, CancellationToken ct = default)
        => CreateCalendarAsync(calendar, ct);

    public async Task<Dictionary<string, Calendar>> GetCalendarLookupAsync(CancellationToken ct = default)
    {
        var calendars = await GetAllAsync(ct).ConfigureAwait(false);
        return calendars.ToDictionary(c => c.Id);
    }

    private static Calendar MapFromDto(CalendarDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        DisplayName = dto.DisplayName,
        Color = dto.Color,
        Type = (CalendarType)dto.Type,
        EnableBlocking = dto.EnableBlocking,
        ReceiveBlocks = dto.ReceiveBlocks,
        ExternalAccountId = dto.ExternalAccountId,
        IsHidden = dto.IsHidden
    };

    // DTO for Dapper mapping since SQLite stores enum as int and bools as int
    private sealed class CalendarDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string Color { get; set; } = string.Empty;
        public int Type { get; set; }
        public bool EnableBlocking { get; set; }
        public bool ReceiveBlocks { get; set; }
        public string? ExternalAccountId { get; set; }
        public bool IsHidden { get; set; }
    }
}
