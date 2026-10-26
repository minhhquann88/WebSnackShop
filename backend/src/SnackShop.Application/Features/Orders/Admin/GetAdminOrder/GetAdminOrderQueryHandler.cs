using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.GetAdminOrder;

public sealed class GetAdminOrderQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetAdminOrderQuery, AdminOrderDetailResponse>
{
    public async Task<Result<AdminOrderDetailResponse>> Handle(GetAdminOrderQuery query, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking()
            .Where(o => o.Id == query.Id)
            .Select(o => new AdminOrderDetailResponse(
                o.Id,
                o.OrderNumber.Value,
                o.Status,
                o.CustomerId,
                o.ContactEmail,
                new AddressResponse(o.ShippingAddress.FullName, o.ShippingAddress.Phone, o.ShippingAddress.Line1,
                    o.ShippingAddress.Ward, o.ShippingAddress.District, o.ShippingAddress.Province),
                o.Items.OrderBy(i => i.ProductName).Select(i => new OrderItemResponse(
                    i.VariantId, i.ProductId, i.ProductName, i.VariantName, i.Sku, i.ImageUrl,
                    new MoneyResponse(i.UnitPrice.Amount, i.UnitPrice.Currency), i.Quantity,
                    new MoneyResponse(i.LineTotal.Amount, i.LineTotal.Currency))).ToList(),
                new MoneyResponse(o.Subtotal.Amount, o.Subtotal.Currency),
                new MoneyResponse(o.ShippingFee.Amount, o.ShippingFee.Currency),
                new MoneyResponse(o.DiscountAmount.Amount, o.DiscountAmount.Currency),
                new MoneyResponse(o.Total.Amount, o.Total.Currency),
                o.CustomerNote,
                o.InternalNote,
                o.PlacedAt,
                o.ConfirmedAt,
                o.ShippedAt,
                o.DeliveredAt,
                o.CancelledAt,
                o.CancelReason,
                o.CancelledBy,
                new AdminPaymentResponse(o.Payment.Method, o.Payment.Status,
                    new MoneyResponse(o.Payment.Amount.Amount, o.Payment.Amount.Currency), o.Payment.Provider,
                    o.Payment.TransactionRef, o.Payment.TransferContent, o.Payment.PaidAt, o.Payment.ConfirmedByUserId),
                o.StatusHistory.OrderBy(h => h.ChangedAt).Select(h => new AdminStatusHistoryResponse(
                    h.FromStatus, h.ToStatus, h.ActorType, h.ChangedByUserId, h.Note, h.ChangedAt)).ToList(),
                new AdminOrderActionsResponse(
                    o.Status == OrderStatus.Pending,
                    o.Status == OrderStatus.Confirmed,
                    o.Status == OrderStatus.Preparing,
                    o.Status == OrderStatus.Shipping,
                    o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled,
                    o.Status != OrderStatus.Cancelled && o.Payment.Status == PaymentStatus.Unpaid,
                    o.Payment.Status == PaymentStatus.RefundPending)))
            .FirstOrDefaultAsync(cancellationToken);

        return order is null ? Result.Failure<AdminOrderDetailResponse>(OrderErrors.NotFound) : order;
    }
}
