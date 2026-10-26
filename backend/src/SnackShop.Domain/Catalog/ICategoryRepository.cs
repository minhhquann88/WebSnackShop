namespace SnackShop.Domain.Catalog;

/// <summary>Write-side access to categories (tracked).</summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Category>> ListAllAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(Slug slug, Guid? excludeId, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken cancellationToken);

    Task<bool> HasProductsAsync(Guid id, CancellationToken cancellationToken);

    void Add(Category category);

    void Remove(Category category);
}
