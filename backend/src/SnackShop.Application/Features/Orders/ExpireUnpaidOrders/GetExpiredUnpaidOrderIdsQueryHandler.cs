using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.ExpireUnpaidOrders;

public sealed class GetExpiredUnpaidOrderIdsQueryHandler(IApplicationDbContext db, TimeProvider timeProvider)
    : IQueryHandler<GetExpiredUnpaidOrderIdsQuery, IReadOnlyList<Guid>>
{
    public async Task<Result<IReadOnlyList<Guid>>> Handle(GetExpiredUnpaidOrderIdsQuery query, CancellationToken cancellationToken)
    {
        var hours = await db.ShopSettings.AsNoTracking().Select(s => (int?)s.UnpaidBankTransferExpiryHours).FirstOrDefaultAsync(cancellationToken);
        if (hours is null)
        {
            return Array.Empty<Guid>();
        }

        var placedBefore = timeProvider.GetUtcNow().AddHours(-hours.Value);
        var ids = await db.Orders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Pending
                && o.PaymentMethod == PaymentMethod.BankTransfer
                && o.Payment.Status == PaymentStatus.Unpaid
                && o.PlacedAt < placedBefore)
            .OrderBy(o => o.PlacedAt)
            .Select(o => o.Id)
            .Take(query.BatchSize)
            .ToListAsync(cancellationToken);

        return ids;
    }
}
