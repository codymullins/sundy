namespace Sundy.Core.Calendars.Outlook;

/// <summary>
/// Store for persisting connected external accounts.
/// </summary>
public interface IConnectedAccountStore
{
    /// <summary>
    /// Get all connected accounts.
    /// </summary>
    Task<List<ConnectedAccount>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a specific account by ID.
    /// </summary>
    Task<ConnectedAccount?> GetByIdAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Add a new connected account.
    /// </summary>
    Task AddAsync(ConnectedAccount account, CancellationToken ct = default);

    /// <summary>
    /// Update an existing connected account.
    /// </summary>
    Task UpdateAsync(ConnectedAccount account, CancellationToken ct = default);

    /// <summary>
    /// Delete a connected account by ID.
    /// </summary>
    Task DeleteAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Get all accounts of a specific provider type.
    /// </summary>
    Task<List<ConnectedAccount>> GetByProviderTypeAsync(ProviderType providerType, CancellationToken ct = default);
}
