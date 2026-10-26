using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Inventory.ReceiveStock;

public sealed class ReceiveStockCommandHandler(IStockItemRepository stockItems, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<ReceiveStockCommand>
{
    public async Task<Result> Handle(ReceiveStockCommand command, CancellationToken cancellationToken)
    {
        var stock = await stockItems.GetByVariantIdAsync(command.VariantId, cancellationToken);
        return stock is null
            ? Result.Failure(InventoryErrors.StockItemNotFound)
            : stock.Receive(command.Quantity, command.Note, new StockContext(timeProvider.GetUtcNow(), currentUser.UserId));
    }
}
