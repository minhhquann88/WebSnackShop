using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.Admin.ShipOrder;

public sealed class ShipOrderCommandHandler(StaffOrderAccess access) : ICommandHandler<ShipOrderCommand>
{
    public async Task<Result> Handle(ShipOrderCommand command, CancellationToken cancellationToken)
    {
        var ctx = await access.LoadAsync(command.OrderId, cancellationToken);
        return ctx.IsFailure ? ctx : ctx.Value.Order.Ship(ctx.Value.Actor, command.Note, ctx.Value.Now);
    }
}
