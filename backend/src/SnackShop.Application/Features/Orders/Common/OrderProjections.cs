using System.Linq.Expressions;
using SnackShop.Application.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Common;

/// <summary>Reusable EF projections so every order query shapes data the same way (translated to SQL).</summary>
public static class OrderProjections
{
    public static readonly Expression<Func<Order, OrderDetailResponse>> Detail = o => new OrderDetailResponse(
        o.Id,
        o.OrderNumber.Value,
        o.Status,
        o.PaymentMethod,
        o.Payment.Status,
        o.Items.OrderBy(i => i.ProductName).Select(i => new OrderItemResponse(
            i.VariantId, i.ProductId, i.ProductName, i.VariantName, i.Sku, i.ImageUrl,
            new MoneyResponse(i.UnitPrice.Amount, i.UnitPrice.Currency), i.Quantity,
            new MoneyResponse(i.LineTotal.Amount, i.LineTotal.Currency))).ToList(),
        new MoneyResponse(o.Subtotal.Amount, o.Subtotal.Currency),
        new MoneyResponse(o.ShippingFee.Amount, o.ShippingFee.Currency),
        new MoneyResponse(o.DiscountAmount.Amount, o.DiscountAmount.Currency),
        new MoneyResponse(o.Total.Amount, o.Total.Currency),
        new AddressResponse(o.ShippingAddress.FullName, o.ShippingAddress.Phone, o.ShippingAddress.Line1,
            o.ShippingAddress.Ward, o.ShippingAddress.District, o.ShippingAddress.Province),
        o.ContactEmail,
        o.CustomerNote,
        o.PlacedAt,
        o.ConfirmedAt,
        o.ShippedAt,
        o.DeliveredAt,
        o.CancelledAt,
        o.CancelReason,
        o.Status == OrderStatus.Pending && o.Payment.Status != PaymentStatus.Paid,
        o.StatusHistory.OrderBy(h => h.ChangedAt).Select(h => new OrderTimelineEntryResponse(h.ToStatus, h.ChangedAt, h.Note)).ToList(),
        null);

    public static readonly Expression<Func<Order, OrderSummaryResponse>> Summary = o => new OrderSummaryResponse(
        o.Id,
        o.OrderNumber.Value,
        o.Status,
        o.PaymentMethod,
        o.Payment.Status,
        new MoneyResponse(o.Total.Amount, o.Total.Currency),
        o.Items.Sum(i => i.Quantity),
        o.Items.OrderBy(i => i.ProductName).Select(i => i.ProductName).FirstOrDefault(),
        o.Items.OrderBy(i => i.ProductName).Select(i => i.ImageUrl).FirstOrDefault(),
        o.PlacedAt);
}
