using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Catalog;
using SnackShop.Application.Features.Cart;
using SnackShop.Domain.Common;
using SnackShop.Domain.Settings;

namespace SnackShop.Application.Features.Orders.EstimateOrder;

public sealed class EstimateOrderQueryHandler(IApplicationDbContext db, PurchasableVariants variants)
    : IQueryHandler<EstimateOrderQuery, OrderEstimateResponse>
{
    public async Task<Result<OrderEstimateResponse>> Handle(EstimateOrderQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.ShopSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (settings is null)
        {
            return Result.Failure<OrderEstimateResponse>(SettingsErrors.NotSeeded);
        }

        var snapshots = await variants.LoadAsync(query.Items.Select(i => i.VariantId).ToList(), cancellationToken);

        var lines = query.Items
            .Where(i => snapshots.ContainsKey(i.VariantId))
            .Select(i => PricedLineResponse.From(snapshots[i.VariantId], i.Quantity))
            .ToList();

        var unavailable = query.Items
            .Where(i => !snapshots.ContainsKey(i.VariantId))
            .Select(i => i.VariantId)
            .Concat(lines.Where(l => !l.IsAvailable).Select(l => l.VariantId))
            .ToList();

        var subtotal = Money.Vnd(lines.Where(l => l.IsAvailable).Sum(l => l.LineTotal.Amount));
        var shipping = subtotal.IsZero ? Money.Zero() : settings.CalculateShippingFee(subtotal);

        return new OrderEstimateResponse(
            lines,
            MoneyResponse.From(subtotal),
            MoneyResponse.From(shipping),
            MoneyResponse.From(subtotal + shipping),
            MoneyResponse.FromNullable(settings.FreeShippingThreshold),
            CanPlaceOrder: unavailable.Count == 0 && lines.Count > 0,
            unavailable);
    }
}
