using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Common.Catalog;

/// <summary>Live catalog data of a variant used by cart, estimate and checkout (prices are never taken from clients).</summary>
public sealed record VariantSnapshot(
    Guid VariantId,
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    string VariantName,
    string Sku,
    string? ImageUrl,
    Money Price,
    Money? CompareAtPrice,
    int Available,
    bool IsPurchasable);

/// <summary>Loads <see cref="VariantSnapshot"/>s in one query. Missing ids are simply absent from the result.</summary>
public sealed class PurchasableVariants(IApplicationDbContext db)
{
    public async Task<IReadOnlyDictionary<Guid, VariantSnapshot>> LoadAsync(IReadOnlyCollection<Guid> variantIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        if (variantIds.Count == 0)
        {
            return new Dictionary<Guid, VariantSnapshot>();
        }

        var rows = await db.Products.AsNoTracking()
            .SelectMany(p => p.Variants, (p, v) => new { p, v })
            .Where(x => variantIds.Contains(x.v.Id))
            .Select(x => new
            {
                VariantId = x.v.Id,
                ProductId = x.p.Id,
                ProductName = x.p.Name,
                ProductSlug = x.p.Slug.Value,
                VariantName = x.v.Name,
                Sku = x.v.Sku.Value,
                ImageUrl = x.p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
                PriceAmount = x.v.Price.Amount,
                PriceCurrency = x.v.Price.Currency,
                CompareAmount = x.v.CompareAtPrice == null ? (decimal?)null : x.v.CompareAtPrice.Amount,
                CompareCurrency = x.v.CompareAtPrice == null ? null : x.v.CompareAtPrice.Currency,
                Available = db.StockItems.Where(s => s.Id == x.v.Id).Select(s => s.QuantityOnHand - s.QuantityReserved).FirstOrDefault(),
                Purchasable = x.p.IsActive && x.v.IsActive && db.Categories.Any(c => c.Id == x.p.CategoryId && c.IsActive),
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            r => r.VariantId,
            r => new VariantSnapshot(
                r.VariantId,
                r.ProductId,
                r.ProductName,
                r.ProductSlug,
                r.VariantName,
                r.Sku,
                r.ImageUrl,
                Money.From(r.PriceAmount, r.PriceCurrency),
                r.CompareAmount is { } amount ? Money.From(amount, r.CompareCurrency!) : null,
                Math.Max(0, r.Available),
                r.Purchasable));
    }
}
