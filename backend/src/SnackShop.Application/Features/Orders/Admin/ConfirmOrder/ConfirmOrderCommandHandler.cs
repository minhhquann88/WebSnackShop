using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.Admin.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler(StaffOrderAccess access, OrderInventory inventory)
    : ICommandHandler<ConfirmOrderCommand>
{
    public async Task<Result> Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        var ctx = await access.LoadAsync(command.OrderId, cancellationToken);
        if (ctx.IsFailure)
        {
            return ctx;
        }

        var (order, actor, now) = ctx.Value;
        var confirmed = order.Confirm(actor, now, command.MarkPaid, command.TransactionRef);
        if (confirmed.IsFailure)
        {
            return confirmed;
        }

        return await inventory.CommitAsync(order, ctx.Value.Stock, cancellationToken);
    }
}
