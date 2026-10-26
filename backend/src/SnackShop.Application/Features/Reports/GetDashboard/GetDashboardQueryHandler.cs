using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Inventory;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Reports.GetDashboard;

public sealed class GetDashboardQueryHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    private const int LowStockListSize = 10;

    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery query, CancellationToken cancellationToken)
    {
        var today = ShopTime.StartOfVietnamDay(timeProvider.GetUtcNow());
        var orders = db.Orders.AsNoTracking();
        var delivered = orders.Where(o => o.Status == OrderStatus.Delivered);

        var revenueToday = await delivered.Where(o => o.DeliveredAt >= today).SumAsync(o => o.Total.Amount, cancellationToken);
        var revenue7 = await delivered.Where(o => o.DeliveredAt >= today.AddDays(-6)).SumAsync(o => o.Total.Amount, cancellationToken);
        var revenue30 = await delivered.Where(o => o.DeliveredAt >= today.AddDays(-29)).SumAsync(o => o.Total.Amount, cancellationToken);
        var placedToday = await orders.CountAsync(o => o.PlacedAt >= today, cancellationToken);
        var unpaidTransfers = await orders.CountAsync(o => o.Status == OrderStatus.Pending
            && o.PaymentMethod == PaymentMethod.BankTransfer && o.Payment.Status == PaymentStatus.Unpaid, cancellationToken);

        var byStatus = await orders.GroupBy(o => o.Status)
            .Select(g => new OrderStatusCountResponse(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var lowStock = db.Products.AsNoTracking()
            .SelectMany(p => p.Variants.Where(v => v.IsActive), (p, v) => new { p, v })
            .Join(db.StockItems, x => x.v.Id, s => s.Id, (x, s) => new { x.p, x.v, s })
            .Where(x => x.p.IsActive && x.s.QuantityOnHand - x.s.QuantityReserved <= x.s.LowStockThreshold);

        var lowStockCount = await lowStock.CountAsync(cancellationToken);
        var lowStockItems = await lowStock
            .OrderBy(x => x.s.QuantityOnHand - x.s.QuantityReserved)
            .Take(LowStockListSize)
            .Select(x => new StockLevelResponse(x.v.Id, x.p.Id, x.p.Name, x.v.Name, x.v.Sku.Value, x.s.QuantityOnHand,
                x.s.QuantityReserved, x.s.QuantityOnHand - x.s.QuantityReserved, x.s.LowStockThreshold, true))
            .ToListAsync(cancellationToken);

        return new DashboardResponse(
            MoneyResponse.From(Money.Vnd(revenueToday)),
            MoneyResponse.From(Money.Vnd(revenue7)),
            MoneyResponse.From(Money.Vnd(revenue30)),
            placedToday,
            byStatus.FirstOrDefault(s => s.Status == OrderStatus.Pending)?.Count ?? 0,
            unpaidTransfers,
            Enum.GetValues<OrderStatus>().Select(s => byStatus.FirstOrDefault(b => b.Status == s) ?? new OrderStatusCountResponse(s, 0)).ToList(),
            lowStockCount,
            lowStockItems);
    }
}
