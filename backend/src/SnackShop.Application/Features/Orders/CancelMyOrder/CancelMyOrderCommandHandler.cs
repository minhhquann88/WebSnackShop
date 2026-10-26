using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.CancelMyOrder;

public sealed class CancelMyOrderCommandHandler(
    IOrderRepository orders,
    ICurrentUser currentUser,
    OrderInventory inventory,
    TimeProvider timeProvider) : ICommandHandler<CancelMyOrderCommand>
{
    private const string DefaultReason = "Khách hàng hủy đơn";

    public async Task<Result> Handle(CancelMyOrderCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return userId;
        }

        var order = await orders.GetByNumberAsync(OrderNumber.FromTrusted(command.OrderNumber), cancellationToken);
        if (order is null || order.CustomerId != userId.Value)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        var now = timeProvider.GetUtcNow();
        var reason = string.IsNullOrWhiteSpace(command.Reason) ? DefaultReason : command.Reason;
        var cancelled = order.Cancel(OrderActor.Customer(userId.Value), reason, now);
        if (cancelled.IsFailure)
        {
            return cancelled;
        }

        return await inventory.ApplyCancellationAsync(order, cancelled.Value, new StockContext(now, userId.Value), cancellationToken);
    }
}
