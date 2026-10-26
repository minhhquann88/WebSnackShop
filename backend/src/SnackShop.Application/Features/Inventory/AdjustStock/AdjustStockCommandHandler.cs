using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Inventory.AdjustStock;

public sealed class AdjustStockCommandHandler(IStockItemRepository stockItems, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<AdjustStockCommand>
{
    public async Task<Result> Handle(AdjustStockCommand command, CancellationToken cancellationToken)
    {
        var stock = await stockItems.GetByVariantIdAsync(command.VariantId, cancellationToken);
        return stock is null
            ? Result.Failure(InventoryErrors.StockItemNotFound)
            : stock.Adjust(command.Delta, command.Reason, new StockContext(timeProvider.GetUtcNow(), currentUser.UserId));
    }
}
