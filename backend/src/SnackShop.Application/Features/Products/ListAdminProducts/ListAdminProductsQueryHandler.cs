using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.ListAdminProducts;

public sealed class ListAdminProductsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListAdminProductsQuery, PagedResponse<AdminProductSummaryResponse>>
{
    public async Task<Result<PagedResponse<AdminProductSummaryResponse>>> Handle(ListAdminProductsQuery query, CancellationToken cancellationToken)
    {
        var products = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = TextNormalizer.ForSearch(query.Search);
            // SKU is a value-converted column, so it supports exact matches only.
            var sku = Sku.FromTrusted(query.Search.Trim().ToUpperInvariant());
            products = products.Where(p => p.SearchText.Contains(term) || p.Variants.Any(v => v.Sku == sku));
        }

        if (query.CategoryId is { } categoryId)
        {
            products = products.Where(p => p.CategoryId == categoryId);
        }

        if (query.IsActive is { } isActive)
        {
            products = products.Where(p => p.IsActive == isActive);
        }

        var page = await products
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminProductSummaryResponse(
                p.Id,
                p.Name,
                p.Slug.Value,
                p.CategoryId,
                db.Categories.IgnoreQueryFilters().Where(c => c.Id == p.CategoryId).Select(c => c.Name).FirstOrDefault() ?? string.Empty,
                p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
                p.Variants.OrderBy(v => v.Price.Amount).Select(v => new MoneyResponse(v.Price.Amount, v.Price.Currency)).FirstOrDefault(),
                p.Variants.Count,
                db.StockItems.Where(s => p.Variants.Any(v => v.Id == s.Id)).Sum(s => s.QuantityOnHand - s.QuantityReserved),
                p.IsActive,
                p.IsFeatured,
                p.SoldCount,
                p.UpdatedAt))
            .ToPagedResponseAsync(query.Page, query.PageSize, cancellationToken);

        return page;
    }
}
