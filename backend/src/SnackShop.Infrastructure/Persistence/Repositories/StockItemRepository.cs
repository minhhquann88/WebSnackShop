using Microsoft.EntityFrameworkCore;
using SnackShop.Domain.Inventory;

namespace SnackShop.Infrastructure.Persistence.Repositories;

/// <summary>Loads stock items without their movement history (movements are append-only and only added).</summary>
internal sealed class StockItemRepository(ApplicationDbContext db) : IStockItemRepository
{
    public Task<StockItem?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken) =>
        db.StockItemSet.FirstOrDefaultAsync(s => s.Id == variantId, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, StockItem>> GetByVariantIdsAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken cancellationToken) =>
        await db.StockItemSet.Where(s => variantIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, cancellationToken);

    public void Add(StockItem stockItem) => db.StockItemSet.Add(stockItem);
}
