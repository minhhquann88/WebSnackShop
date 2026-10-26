using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.Admin.StartPreparingOrder;

public sealed class StartPreparingOrderCommandHandler(StaffOrderAccess access) : ICommandHandler<StartPreparingOrderCommand>
{
    public async Task<Result> Handle(StartPreparingOrderCommand command, CancellationToken cancellationToken)
    {
        var ctx = await access.LoadAsync(command.OrderId, cancellationToken);
        return ctx.IsFailure ? ctx : ctx.Value.Order.StartPreparing(ctx.Value.Actor, ctx.Value.Now);
    }
}
