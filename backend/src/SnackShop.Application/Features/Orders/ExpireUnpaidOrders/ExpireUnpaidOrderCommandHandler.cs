using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.ExpireUnpaidOrders;

public sealed class ExpireUnpaidOrderCommandHandler(IOrderRepository orders, OrderInventory inventory, TimeProvider timeProvider)
    : ICommandHandler<ExpireUnpaidOrderCommand, bool>
{
    public const string Reason = "Quá hạn thanh toán chuyển khoản";

    public async Task<Result<bool>> Handle(ExpireUnpaidOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(command.OrderId, cancellationToken);

        // Re-check under this transaction: staff may have confirmed payment since the candidate list was read.
        if (order is null || order.Status != OrderStatus.Pending || order.Payment.Status != PaymentStatus.Unpaid)
        {
            return false;
        }

        var now = timeProvider.GetUtcNow();
        var cancelled = order.Cancel(OrderActor.System, Reason, now);
        if (cancelled.IsFailure)
        {
            return Result.Failure<bool>(cancelled.Error);
        }

        var released = await inventory.ApplyCancellationAsync(order, cancelled.Value, new StockContext(now, null), cancellationToken);
        return released.IsFailure ? Result.Failure<bool>(released.Error) : true;
    }
}
