namespace SnackShop.Domain.Catalog;

/// <summary>Write-side access to products with variants and images loaded.</summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>Loads the products owning the given variants (any state, including soft-deleted).</summary>
    Task<IReadOnlyList<Product>> GetByVariantIdsAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(Slug slug, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>True when a variant of another product (or another variant) uses the SKU.</summary>
    Task<bool> SkuExistsAsync(Sku sku, Guid? excludeVariantId, CancellationToken cancellationToken);

    void Add(Product product);

    void Remove(Product product);
}
