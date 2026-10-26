using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.Admin.MarkOrderPaid;

public sealed class MarkOrderPaidCommandHandler(StaffOrderAccess access) : ICommandHandler<MarkOrderPaidCommand>
{
    public async Task<Result> Handle(MarkOrderPaidCommand command, CancellationToken cancellationToken)
    {
        var ctx = await access.LoadAsync(command.OrderId, cancellationToken);
        return ctx.IsFailure
            ? ctx
            : ctx.Value.Order.MarkPaid(ctx.Value.Actor, command.TransactionRef, command.PaidAt?.ToUniversalTime() ?? ctx.Value.Now, ctx.Value.Now);
    }
}
