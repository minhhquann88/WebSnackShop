using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.Admin.MarkOrderRefunded;

public sealed class MarkOrderRefundedCommandHandler(StaffOrderAccess access) : ICommandHandler<MarkOrderRefundedCommand>
{
    public async Task<Result> Handle(MarkOrderRefundedCommand command, CancellationToken cancellationToken)
    {
        var ctx = await access.LoadAsync(command.OrderId, cancellationToken);
        return ctx.IsFailure
            ? ctx
            : ctx.Value.Order.MarkRefunded(ctx.Value.Actor, command.TransactionRef, ctx.Value.Now);
    }
}
