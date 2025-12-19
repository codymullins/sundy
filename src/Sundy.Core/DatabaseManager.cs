using Microsoft.EntityFrameworkCore;

namespace Sundy.Core;

// public class DatabaseManager(SundyDbContext dbContext)
// {
//     public async Task<bool> DatabaseExistsAsync(CancellationToken ct = default)
//     {
//         return await dbContext.Database.CanConnectAsync(ct).ConfigureAwait(false);
//     }
//
//     public async Task InitializeDatabaseAsync(CancellationToken ct = default)
//     {
//         await dbContext.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);
//     }
//
//     public async Task DeleteDatabaseAsync(CancellationToken ct = default)
//     {
//         await dbContext.Database.EnsureDeletedAsync(ct).ConfigureAwait(false);
//     }
// }
