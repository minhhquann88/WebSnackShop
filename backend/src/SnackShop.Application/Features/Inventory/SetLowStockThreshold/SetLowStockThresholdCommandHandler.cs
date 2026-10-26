using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Inventory.SetLowStockThreshold;

public sealed class SetLowStockThresholdCommandHandler(IStockItemRepository stockItems)
    : ICommandHandler<SetLowStockThresholdCommand>
{
    public async Task<Result> Handle(SetLowStockThresholdCommand command, CancellationToken cancellationToken)
    {
        var stock = await stockItems.GetByVariantIdAsync(command.VariantId, cancellationToken);
        return stock is null
            ? Result.Failure(InventoryErrors.StockItemNotFound)
            : stock.SetLowStockThreshold(command.Threshold);
    }
}
