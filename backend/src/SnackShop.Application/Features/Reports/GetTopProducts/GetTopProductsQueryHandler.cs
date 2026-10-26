using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Reports.GetTopProducts;

public sealed class GetTopProductsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetTopProductsQuery, IReadOnlyList<TopProductResponse>>
{
    public async Task<Result<IReadOnlyList<TopProductResponse>>> Handle(GetTopProductsQuery query, CancellationToken cancellationToken)
    {
        var start = new DateTimeOffset(query.From.ToDateTime(TimeOnly.MinValue), ShopTime.VietnamOffset).ToUniversalTime();
        var end = new DateTimeOffset(query.To.AddDays(1).ToDateTime(TimeOnly.MinValue), ShopTime.VietnamOffset).ToUniversalTime();

        var top = await db.Orders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered && o.DeliveredAt >= start && o.DeliveredAt < end)
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new { g.Key.ProductId, g.Key.ProductName, Quantity = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.LineTotal.Amount) })
            .OrderByDescending(x => x.Quantity).ThenByDescending(x => x.Revenue)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        return top.Select(x => new TopProductResponse(x.ProductId, x.ProductName, x.Quantity, new MoneyResponse(x.Revenue, Money.DefaultCurrency))).ToList();
    }
}
