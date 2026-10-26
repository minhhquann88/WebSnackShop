using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.TrackOrder;

public sealed class TrackOrderQueryHandler(IApplicationDbContext db, BankTransferInstructions bankTransfer)
    : IQueryHandler<TrackOrderQuery, OrderDetailResponse>
{
    public async Task<Result<OrderDetailResponse>> Handle(TrackOrderQuery query, CancellationToken cancellationToken)
    {
        var number = OrderNumber.FromTrusted(query.OrderNumber);
        var phone = query.Phone.Trim();

        var order = await db.Orders.AsNoTracking()
            .Where(o => o.OrderNumber == number && o.ShippingAddress.Phone == phone)
            .Select(OrderProjections.Detail)
            .FirstOrDefaultAsync(cancellationToken);

        // Same error whether the number or the phone is wrong, so neither can be probed.
        return order is null
            ? Result.Failure<OrderDetailResponse>(OrderErrors.NotFound)
            : await bankTransfer.AttachAsync(order, cancellationToken);
    }
}
