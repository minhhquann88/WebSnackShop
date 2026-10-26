using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.GetProductBySlug;

public sealed class GetProductBySlugQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetProductBySlugQuery, ProductDetailResponse>
{
    public async Task<Result<ProductDetailResponse>> Handle(GetProductBySlugQuery query, CancellationToken cancellationToken)
    {
        var slug = Slug.FromTrusted(query.Slug.Trim().ToLowerInvariant());

        var product = await db.Products.AsNoTracking()
            .Where(p => p.Slug == slug && p.IsActive)
            .Join(db.Categories.Where(c => c.IsActive), p => p.CategoryId, c => c.Id, (p, c) => new { p, c })
            .Select(x => new ProductDetailResponse(
                x.p.Id,
                x.p.Name,
                x.p.Slug.Value,
                x.p.ShortDescription,
                x.p.Description,
                x.p.Ingredients,
                x.p.Origin,
                x.p.Brand,
                x.p.ShelfLifeDays,
                x.p.SoldCount,
                new ProductCategoryResponse(x.c.Id, x.c.Name, x.c.Slug.Value),
                x.p.Images.OrderBy(i => i.SortOrder)
                    .Select(i => new ProductImageResponse(i.Id, i.Url, i.AltText, i.SortOrder, i.IsPrimary)).ToList(),
                x.p.Variants.Where(v => v.IsActive).OrderBy(v => v.SortOrder).ThenBy(v => v.Price.Amount)
                    .Select(v => new ProductVariantResponse(
                        v.Id,
                        v.Name,
                        v.Sku.Value,
                        new MoneyResponse(v.Price.Amount, v.Price.Currency),
                        v.CompareAtPrice == null ? null : new MoneyResponse(v.CompareAtPrice.Amount, v.CompareAtPrice.Currency),
                        v.WeightGrams,
                        db.StockItems.Where(s => s.Id == v.Id)
                            .Select(s => s.QuantityOnHand - s.QuantityReserved).FirstOrDefault()))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? Result.Failure<ProductDetailResponse>(CatalogErrors.ProductNotFound) : product;
    }
}
