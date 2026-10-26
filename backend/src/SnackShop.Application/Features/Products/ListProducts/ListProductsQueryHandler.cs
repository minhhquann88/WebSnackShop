using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.ListProducts;

public sealed class ListProductsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListProductsQuery, PagedResponse<ProductSummaryResponse>>
{
    public async Task<Result<PagedResponse<ProductSummaryResponse>>> Handle(ListProductsQuery query, CancellationToken cancellationToken)
    {
        var products = db.Products.AsNoTracking()
            .Where(p => p.IsActive && db.Categories.Any(c => c.Id == p.CategoryId && c.IsActive));

        if (!string.IsNullOrWhiteSpace(query.CategorySlug))
        {
            var slug = Slug.FromTrusted(query.CategorySlug.Trim().ToLowerInvariant());
            products = products.Where(p => db.Categories.Any(c => c.Id == p.CategoryId && c.Slug == slug));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = TextNormalizer.ForSearch(query.Search);
            products = products.Where(p => p.SearchText.Contains(term));
        }

        if (query.Featured is { } featured)
        {
            products = products.Where(p => p.IsFeatured == featured);
        }

        products = (query.Sort ?? "-createdAt") switch
        {
            "createdAt" => products.OrderBy(p => p.CreatedAt),
            "price" => products.OrderBy(p => p.Variants.Where(v => v.IsActive).Min(v => (decimal?)v.Price.Amount)).ThenBy(p => p.Name),
            "-price" => products.OrderByDescending(p => p.Variants.Where(v => v.IsActive).Min(v => (decimal?)v.Price.Amount)).ThenBy(p => p.Name),
            "soldCount" => products.OrderBy(p => p.SoldCount).ThenBy(p => p.Name),
            "-soldCount" => products.OrderByDescending(p => p.SoldCount).ThenBy(p => p.Name),
            "name" => products.OrderBy(p => p.Name),
            "-name" => products.OrderByDescending(p => p.Name),
            _ => products.OrderByDescending(p => p.CreatedAt),
        };

        // Scalar correlated subqueries (index on product_variants.product_id / stock_items PK) instead of
        // window functions over the whole variants table; prices are VND, so currency is not projected.
        var page = await products
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.ShortDescription,
                ImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault(),
                MinPrice = p.Variants.Where(v => v.IsActive).Min(v => (decimal?)v.Price.Amount),
                CompareAt = p.Variants.Where(v => v.IsActive).OrderBy(v => v.Price.Amount)
                    .Select(v => v.CompareAtPrice == null ? (decimal?)null : v.CompareAtPrice.Amount).FirstOrDefault(),
                p.SoldCount,
                p.IsFeatured,
                InStock = p.Variants.Any(v => v.IsActive
                    && db.StockItems.Any(s => s.Id == v.Id && s.QuantityOnHand - s.QuantityReserved > 0)),
            })
            .ToPagedResponseAsync(query.Page, query.PageSize, cancellationToken);

        var items = page.Items.Select(p => new ProductSummaryResponse(
                p.Id,
                p.Name,
                p.Slug.Value,
                p.ShortDescription,
                p.ImageUrl,
                p.MinPrice is { } min ? new MoneyResponse(min, Money.DefaultCurrency) : null,
                p.CompareAt is { } compare ? new MoneyResponse(compare, Money.DefaultCurrency) : null,
                p.SoldCount,
                p.IsFeatured,
                p.InStock))
            .ToList();

        return new PagedResponse<ProductSummaryResponse>(items, page.Page, page.PageSize, page.TotalCount);
    }
}
