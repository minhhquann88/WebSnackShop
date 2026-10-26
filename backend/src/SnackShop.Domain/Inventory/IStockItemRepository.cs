namespace SnackShop.Domain.Inventory;

/// <summary>Write-side access to stock items (tracked, with concurrency token).</summary>
public interface IStockItemRepository
{
    Task<StockItem?> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, StockItem>> GetByVariantIdsAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken cancellationToken);

    void Add(StockItem stockItem);
}
