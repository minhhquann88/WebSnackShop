using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.Admin.UpdateInternalNote;

public sealed class UpdateInternalNoteCommandHandler(StaffOrderAccess access) : ICommandHandler<UpdateInternalNoteCommand>
{
    public async Task<Result> Handle(UpdateInternalNoteCommand command, CancellationToken cancellationToken)
    {
        var ctx = await access.LoadAsync(command.OrderId, cancellationToken);
        if (ctx.IsFailure)
        {
            return ctx;
        }

        ctx.Value.Order.SetInternalNote(command.Note, ctx.Value.Now);
        return Result.Success();
    }
}
