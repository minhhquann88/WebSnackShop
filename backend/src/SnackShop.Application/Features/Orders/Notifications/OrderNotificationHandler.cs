using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common.Email;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Notifications;

/// <summary>Emails the customer when an order is placed or changes status. Runs after the transaction commits.</summary>
public sealed class OrderNotificationHandler(
    IApplicationDbContext db,
    IEmailSender emailSender,
    IAppLinks links,
    BankTransferInstructions bankTransfer) :
    IDomainEventHandler<OrderPlacedDomainEvent>,
    IDomainEventHandler<OrderConfirmedDomainEvent>,
    IDomainEventHandler<OrderShippedDomainEvent>,
    IDomainEventHandler<OrderDeliveredDomainEvent>,
    IDomainEventHandler<OrderCancelledDomainEvent>
{
    public async Task Handle(DomainEventNotification<OrderPlacedDomainEvent> notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var order = await LoadAsync(notification.DomainEvent.OrderId, cancellationToken);
        if (order is null)
        {
            return;
        }

        order = await bankTransfer.AttachAsync(order, cancellationToken);
        var rows = order.Items.Select(i => ($"{i.ProductName} ({i.VariantName}) × {i.Quantity}", EmailTemplates.Money(i.LineTotal.Amount)))
            .Append(("Phí giao hàng", EmailTemplates.Money(order.ShippingFee.Amount)))
            .Append(("Tổng cộng", EmailTemplates.Money(order.Total.Amount)));

        var body = EmailTemplates.Paragraph($"Cảm ơn {order.ShippingAddress.FullName}! Chúng tôi đã nhận đơn hàng {order.OrderNumber}.")
            + EmailTemplates.Table(rows);

        if (order.BankTransfer is { } bt)
        {
            body += EmailTemplates.Paragraph($"Vui lòng chuyển khoản {EmailTemplates.Money(bt.Amount.Amount)} tới {bt.BankName} - {bt.AccountNumber} ({bt.AccountName}) với nội dung: {bt.TransferContent}.")
                + EmailTemplates.Image(bt.QrImageUrl, "Mã QR chuyển khoản")
                + EmailTemplates.Paragraph($"Đơn sẽ tự hủy nếu chưa thanh toán trước {ShopTimeText(bt.PayBefore)}.");
        }

        await SendAsync(order, $"Đã nhận đơn hàng {order.OrderNumber}", "Đặt hàng thành công", body, cancellationToken);
    }

    public Task Handle(DomainEventNotification<OrderConfirmedDomainEvent> notification, CancellationToken cancellationToken) =>
        SendStatusAsync(notification?.DomainEvent.OrderId, "Đơn hàng đã được xác nhận", "Shop đã xác nhận đơn và đang chuẩn bị hàng.", cancellationToken);

    public Task Handle(DomainEventNotification<OrderShippedDomainEvent> notification, CancellationToken cancellationToken) =>
        SendStatusAsync(notification?.DomainEvent.OrderId, "Đơn hàng đang được giao", "Đơn hàng đã được giao cho đơn vị vận chuyển.", cancellationToken);

    public Task Handle(DomainEventNotification<OrderDeliveredDomainEvent> notification, CancellationToken cancellationToken) =>
        SendStatusAsync(notification?.DomainEvent.OrderId, "Giao hàng thành công", "Cảm ơn bạn đã mua sắm. Chúc bạn ngon miệng!", cancellationToken);

    public Task Handle(DomainEventNotification<OrderCancelledDomainEvent> notification, CancellationToken cancellationToken) =>
        SendStatusAsync(notification?.DomainEvent.OrderId, "Đơn hàng đã bị hủy", $"Lý do: {notification?.DomainEvent.Reason}", cancellationToken);

    private async Task SendStatusAsync(Guid? orderId, string title, string message, CancellationToken cancellationToken)
    {
        if (orderId is null || await LoadAsync(orderId.Value, cancellationToken) is not { } order)
        {
            return;
        }

        var body = EmailTemplates.Paragraph($"Đơn hàng {order.OrderNumber}: {message}")
            + EmailTemplates.Button("Xem đơn hàng", links.Frontend($"tra-cuu-don?orderNumber={order.OrderNumber}"));
        await SendAsync(order, $"{title} - {order.OrderNumber}", title, body, cancellationToken);
    }

    private ValueTask SendAsync(OrderDetailResponse order, string subject, string title, string body, CancellationToken cancellationToken) =>
        emailSender.QueueAsync(new EmailMessage(order.ContactEmail, subject, EmailTemplates.Layout(links.ShopName, title, body)), cancellationToken);

    private Task<OrderDetailResponse?> LoadAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(OrderProjections.Detail).FirstOrDefaultAsync(cancellationToken);

    private static string ShopTimeText(DateTimeOffset instant) =>
        Domain.Common.ShopTime.ToVietnam(instant).ToString("HH:mm dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
}
