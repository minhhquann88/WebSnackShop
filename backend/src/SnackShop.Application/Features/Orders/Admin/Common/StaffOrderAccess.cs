using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.Common;

/// <summary>Loads an order for a staff action and supplies the staff actor and clock.</summary>
public sealed class StaffOrderAccess(IOrderRepository orders, ICurrentUser currentUser, TimeProvider timeProvider)
{
    public sealed record Context(Order Order, OrderActor Actor, DateTimeOffset Now)
    {
        public StockContext Stock => new(Now, Actor.UserId);
    }

    public async Task<Result<Context>> LoadAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return Result.Failure<Context>(userId.Error);
        }

        var order = await orders.GetByIdAsync(orderId, cancellationToken);
        return order is null
            ? Result.Failure<Context>(OrderErrors.NotFound)
            : new Context(order, OrderActor.Staff(userId.Value), timeProvider.GetUtcNow());
    }
}
