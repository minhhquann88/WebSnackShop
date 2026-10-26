using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Inventory.ListStock;

public sealed class ListStockQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListStockQuery, PagedResponse<StockLevelResponse>>
{
    public async Task<Result<PagedResponse<StockLevelResponse>>> Handle(ListStockQuery query, CancellationToken cancellationToken)
    {
        var rows = db.Products.AsNoTracking()
            .SelectMany(p => p.Variants, (p, v) => new { p, v })
            .Join(db.StockItems, x => x.v.Id, s => s.Id, (x, s) => new { x.p, x.v, s });

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = TextNormalizer.ForSearch(query.Search);
            var sku = Sku.FromTrusted(query.Search.Trim().ToUpperInvariant());
            rows = rows.Where(x => x.p.SearchText.Contains(term) || x.v.Sku == sku);
        }

        if (query.LowStockOnly)
        {
            rows = rows.Where(x => x.s.QuantityOnHand - x.s.QuantityReserved <= x.s.LowStockThreshold);
        }

        var page = await rows
            .OrderBy(x => x.s.QuantityOnHand - x.s.QuantityReserved).ThenBy(x => x.p.Name).ThenBy(x => x.v.Name)
            .Select(x => new StockLevelResponse(
                x.v.Id,
                x.p.Id,
                x.p.Name,
                x.v.Name,
                x.v.Sku.Value,
                x.s.QuantityOnHand,
                x.s.QuantityReserved,
                x.s.QuantityOnHand - x.s.QuantityReserved,
                x.s.LowStockThreshold,
                x.s.QuantityOnHand - x.s.QuantityReserved <= x.s.LowStockThreshold))
            .ToPagedResponseAsync(query.Page, query.PageSize, cancellationToken);

        return page;
    }
}
