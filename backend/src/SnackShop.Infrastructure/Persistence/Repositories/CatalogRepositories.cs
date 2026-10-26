using Microsoft.EntityFrameworkCore;
using SnackShop.Domain.Catalog;

namespace SnackShop.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(ApplicationDbContext db) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.CategorySet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> ListAllAsync(CancellationToken cancellationToken) =>
        await db.CategorySet.ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.CategorySet.AnyAsync(c => c.Id == id, cancellationToken);

    // Unique indexes cover soft-deleted rows too, so uniqueness checks must ignore the soft-delete filter.
    public Task<bool> SlugExistsAsync(Slug slug, Guid? excludeId, CancellationToken cancellationToken) =>
        db.CategorySet.IgnoreQueryFilters().AnyAsync(c => c.Slug == slug && c.Id != excludeId, cancellationToken);

    public Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken cancellationToken) =>
        db.CategorySet.IgnoreQueryFilters().AnyAsync(c => c.Name == name && c.Id != excludeId, cancellationToken);

    public Task<bool> HasProductsAsync(Guid id, CancellationToken cancellationToken) =>
        db.ProductSet.AnyAsync(p => p.CategoryId == id, cancellationToken);

    public void Add(Category category) => db.CategorySet.Add(category);

    public void Remove(Category category) => db.CategorySet.Remove(category);
}

internal sealed class ProductRepository(ApplicationDbContext db) : IProductRepository
{
    private IQueryable<Product> WithChildren => db.ProductSet.Include(p => p.Variants).Include(p => p.Images).AsSplitQuery();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        WithChildren.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await WithChildren.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByVariantIdsAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken cancellationToken) =>
        await WithChildren.IgnoreQueryFilters()
            .Where(p => p.Variants.Any(v => variantIds.Contains(v.Id)))
            .ToListAsync(cancellationToken);

    public Task<bool> SlugExistsAsync(Slug slug, Guid? excludeId, CancellationToken cancellationToken) =>
        db.ProductSet.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug && p.Id != excludeId, cancellationToken);

    public Task<bool> SkuExistsAsync(Sku sku, Guid? excludeVariantId, CancellationToken cancellationToken) =>
        db.Set<ProductVariant>().AnyAsync(v => v.Sku == sku && v.Id != excludeVariantId, cancellationToken);

    public void Add(Product product) => db.ProductSet.Add(product);

    public void Remove(Product product) => db.ProductSet.Remove(product);
}
