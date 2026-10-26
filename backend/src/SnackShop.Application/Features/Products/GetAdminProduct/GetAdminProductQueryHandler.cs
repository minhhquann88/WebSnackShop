using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.GetAdminProduct;

public sealed class GetAdminProductQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetAdminProductQuery, AdminProductDetailResponse>
{
    public async Task<Result<AdminProductDetailResponse>> Handle(GetAdminProductQuery query, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking()
            .Where(p => p.Id == query.Id)
            .Select(p => new AdminProductDetailResponse(
                p.Id,
                p.CategoryId,
                p.Name,
                p.Slug.Value,
                p.ShortDescription,
                p.Description,
                p.Ingredients,
                p.Origin,
                p.Brand,
                p.ShelfLifeDays,
                p.IsActive,
                p.IsFeatured,
                p.SoldCount,
                p.Images.OrderBy(i => i.SortOrder)
                    .Select(i => new ProductImageResponse(i.Id, i.Url, i.AltText, i.SortOrder, i.IsPrimary)).ToList(),
                p.Variants.OrderBy(v => v.SortOrder).ThenBy(v => v.Name)
                    .Select(v => new AdminVariantResponse(
                        v.Id,
                        v.Name,
                        v.Sku.Value,
                        new MoneyResponse(v.Price.Amount, v.Price.Currency),
                        v.CompareAtPrice == null ? null : new MoneyResponse(v.CompareAtPrice.Amount, v.CompareAtPrice.Currency),
                        v.WeightGrams,
                        v.SortOrder,
                        v.IsActive,
                        db.StockItems.Where(s => s.Id == v.Id).Select(s => s.QuantityOnHand).FirstOrDefault(),
                        db.StockItems.Where(s => s.Id == v.Id).Select(s => s.QuantityReserved).FirstOrDefault(),
                        db.StockItems.Where(s => s.Id == v.Id).Select(s => s.LowStockThreshold).FirstOrDefault()))
                    .ToList(),
                p.CreatedAt,
                p.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? Result.Failure<AdminProductDetailResponse>(CatalogErrors.ProductNotFound) : product;
    }
}
