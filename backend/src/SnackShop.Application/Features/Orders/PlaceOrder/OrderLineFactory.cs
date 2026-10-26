using SnackShop.Application.Common.Catalog;
using SnackShop.Application.Features.Cart;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.PlaceOrder;

/// <summary>Turns requested lines into priced order-line snapshots, rejecting anything not purchasable.</summary>
public sealed class OrderLineFactory(PurchasableVariants variants)
{
    public async Task<Result<IReadOnlyList<OrderLine>>> BuildAsync(IReadOnlyList<CartLineRequest> items, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(items);
        var snapshots = await variants.LoadAsync(items.Select(i => i.VariantId).ToList(), cancellationToken);
        var lines = new List<OrderLine>(items.Count);

        foreach (var item in items)
        {
            if (!snapshots.TryGetValue(item.VariantId, out var v) || !v.IsPurchasable)
            {
                return Result.Failure<IReadOnlyList<OrderLine>>(OrderErrors.ProductUnavailable);
            }

            lines.Add(new OrderLine(v.VariantId, v.ProductId, v.ProductName, v.VariantName, v.Sku, v.ImageUrl, v.Price, item.Quantity));
        }

        return lines;
    }
}
