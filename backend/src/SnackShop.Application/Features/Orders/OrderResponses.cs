using SnackShop.Application.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders;

public sealed record OrderItemResponse(
    Guid VariantId,
    Guid ProductId,
    string ProductName,
    string VariantName,
    string Sku,
    string? ImageUrl,
    MoneyResponse UnitPrice,
    int Quantity,
    MoneyResponse LineTotal);

public sealed record OrderTimelineEntryResponse(OrderStatus Status, DateTimeOffset ChangedAt, string? Note);

/// <summary>How to pay a bank-transfer order. <see cref="QrImageUrl"/> is a VietQR image with amount and content prefilled.</summary>
public sealed record BankTransferInstructionsResponse(
    string BankName,
    string AccountNumber,
    string AccountName,
    MoneyResponse Amount,
    string TransferContent,
    string QrImageUrl,
    DateTimeOffset PayBefore);

/// <summary>Customer-facing order view (my orders, guest tracking).</summary>
public sealed record OrderDetailResponse(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    IReadOnlyList<OrderItemResponse> Items,
    MoneyResponse Subtotal,
    MoneyResponse ShippingFee,
    MoneyResponse DiscountAmount,
    MoneyResponse Total,
    AddressResponse ShippingAddress,
    string ContactEmail,
    string? CustomerNote,
    DateTimeOffset PlacedAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? ShippedAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset? CancelledAt,
    string? CancelReason,
    bool CanCancel,
    IReadOnlyList<OrderTimelineEntryResponse> Timeline,
    BankTransferInstructionsResponse? BankTransfer);

public sealed record OrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    MoneyResponse Total,
    int ItemCount,
    string? FirstItemName,
    string? FirstItemImageUrl,
    DateTimeOffset PlacedAt);

public sealed record AdminOrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    MoneyResponse Total,
    int ItemCount,
    string CustomerName,
    string Phone,
    string ContactEmail,
    Guid? CustomerId,
    DateTimeOffset PlacedAt);

public sealed record AdminStatusHistoryResponse(
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    ActorType ActorType,
    Guid? ChangedByUserId,
    string? Note,
    DateTimeOffset ChangedAt);

public sealed record AdminPaymentResponse(
    PaymentMethod Method,
    PaymentStatus Status,
    MoneyResponse Amount,
    string Provider,
    string? TransactionRef,
    string? TransferContent,
    DateTimeOffset? PaidAt,
    Guid? ConfirmedByUserId);

/// <summary>Allowed next actions, so the admin UI never re-implements the state machine.</summary>
public sealed record AdminOrderActionsResponse(
    bool CanConfirm,
    bool CanStartPreparing,
    bool CanShip,
    bool CanDeliver,
    bool CanCancel,
    bool CanMarkPaid,
    bool CanMarkRefunded);

public sealed record AdminOrderDetailResponse(
    Guid Id,
    string OrderNumber,
    OrderStatus Status,
    Guid? CustomerId,
    string ContactEmail,
    AddressResponse ShippingAddress,
    IReadOnlyList<OrderItemResponse> Items,
    MoneyResponse Subtotal,
    MoneyResponse ShippingFee,
    MoneyResponse DiscountAmount,
    MoneyResponse Total,
    string? CustomerNote,
    string? InternalNote,
    DateTimeOffset PlacedAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? ShippedAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset? CancelledAt,
    string? CancelReason,
    ActorType? CancelledBy,
    AdminPaymentResponse Payment,
    IReadOnlyList<AdminStatusHistoryResponse> History,
    AdminOrderActionsResponse Actions);
