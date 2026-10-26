using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.Admin.CancelOrder;

public sealed class CancelOrderCommandHandler(StaffOrderAccess access, OrderInventory inventory)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var ctx = await access.LoadAsync(command.OrderId, cancellationToken);
        if (ctx.IsFailure)
        {
            return ctx;
        }

        var cancelled = ctx.Value.Order.Cancel(ctx.Value.Actor, command.Reason, ctx.Value.Now);
        if (cancelled.IsFailure)
        {
            return cancelled;
        }

        return await inventory.ApplyCancellationAsync(ctx.Value.Order, cancelled.Value, ctx.Value.Stock, cancellationToken);
    }
}
