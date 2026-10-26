using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Reports.GetRevenueReport;

public sealed class GetRevenueReportQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetRevenueReportQuery, RevenueReportResponse>
{
    public async Task<Result<RevenueReportResponse>> Handle(GetRevenueReportQuery query, CancellationToken cancellationToken)
    {
        var start = new DateTimeOffset(query.From.ToDateTime(TimeOnly.MinValue), ShopTime.VietnamOffset).ToUniversalTime();
        var end = new DateTimeOffset(query.To.AddDays(1).ToDateTime(TimeOnly.MinValue), ShopTime.VietnamOffset).ToUniversalTime();

        // Small shop volumes: load (timestamp, amount) pairs and group by Vietnam day in memory,
        // which avoids provider-specific time zone SQL.
        var rows = await db.Orders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered && o.DeliveredAt >= start && o.DeliveredAt < end)
            .Select(o => new { DeliveredAt = o.DeliveredAt!.Value, o.Total.Amount })
            .ToListAsync(cancellationToken);

        var byDay = rows
            .GroupBy(r => DateOnly.FromDateTime(ShopTime.ToVietnam(r.DeliveredAt).DateTime))
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Sum: g.Sum(r => r.Amount)));

        var days = Enumerable.Range(0, query.To.DayNumber - query.From.DayNumber + 1)
            .Select(offset => query.From.AddDays(offset))
            .Select(d => byDay.TryGetValue(d, out var v)
                ? new RevenuePointResponse(d, v.Count, new MoneyResponse(v.Sum, Money.DefaultCurrency))
                : new RevenuePointResponse(d, 0, new MoneyResponse(0m, Money.DefaultCurrency)))
            .ToList();

        return new RevenueReportResponse(query.From, query.To, rows.Count,
            new MoneyResponse(rows.Sum(r => r.Amount), Money.DefaultCurrency), days);
    }
}
