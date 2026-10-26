using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.GetMyOrder;

public sealed class GetMyOrderQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, BankTransferInstructions bankTransfer)
    : IQueryHandler<GetMyOrderQuery, OrderDetailResponse>
{
    public async Task<Result<OrderDetailResponse>> Handle(GetMyOrderQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return Result.Failure<OrderDetailResponse>(userId.Error);
        }

        var number = OrderNumber.FromTrusted(query.OrderNumber);
        var order = await db.Orders.AsNoTracking()
            .Where(o => o.OrderNumber == number && o.CustomerId == userId.Value)
            .Select(OrderProjections.Detail)
            .FirstOrDefaultAsync(cancellationToken);

        return order is null
            ? Result.Failure<OrderDetailResponse>(OrderErrors.NotFound)
            : await bankTransfer.AttachAsync(order, cancellationToken);
    }
}
