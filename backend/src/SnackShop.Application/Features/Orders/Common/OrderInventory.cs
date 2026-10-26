using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Common;

/// <summary>
/// Applies the stock side of order transitions (docs/adr/0008): reserve on place, commit on confirm,
/// release or return on cancel. Stock items are loaded tracked so the unit of work commits them with the order.
/// </summary>
public sealed class OrderInventory(IStockItemRepository stockItems)
{
    public async Task<Result> ReserveAsync(Order order, StockContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        var stocks = await LoadAsync(order, cancellationToken);

        foreach (var item in order.Items)
        {
            if (!stocks.TryGetValue(item.VariantId, out var stock))
            {
                return Result.Failure(InventoryErrors.InsufficientStock(item.Sku, 0));
            }

            var reserved = stock.Reserve(item.Quantity, item.Sku, order.Id, context);
            if (reserved.IsFailure)
            {
                return reserved;
            }
        }

        return Result.Success();
    }

    public Task<Result> CommitAsync(Order order, StockContext context, CancellationToken cancellationToken) =>
        ApplyAsync(order, (stock, item) => stock.CommitReservation(item.Quantity, order.Id, context), cancellationToken);

    public Task<Result> ApplyCancellationAsync(Order order, CancelStockEffect effect, StockContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        return effect switch
        {
            CancelStockEffect.ReleaseReservation =>
                ApplyAsync(order, (stock, item) => stock.ReleaseReservation(item.Quantity, order.Id, context), cancellationToken),
            CancelStockEffect.ReturnToStock =>
                ApplyAsync(order, (stock, item) => stock.Return(item.Quantity, order.Id, context), cancellationToken),
            _ => Task.FromResult(Result.Success()),
        };
    }

    private async Task<Result> ApplyAsync(Order order, Func<StockItem, OrderItem, Result> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        var stocks = await LoadAsync(order, cancellationToken);

        foreach (var item in order.Items)
        {
            // A stock record can be missing only if data was edited by hand; skip rather than block the order.
            if (stocks.TryGetValue(item.VariantId, out var stock))
            {
                var applied = action(stock, item);
                if (applied.IsFailure)
                {
                    return applied;
                }
            }
        }

        return Result.Success();
    }

    private Task<IReadOnlyDictionary<Guid, StockItem>> LoadAsync(Order order, CancellationToken cancellationToken) =>
        stockItems.GetByVariantIdsAsync(order.Items.Select(i => i.VariantId).Distinct().ToList(), cancellationToken);
}
