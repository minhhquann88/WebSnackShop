using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;

namespace SnackShop.Domain.Ordering;

/// <summary>
/// A customer order. Aggregate root owning items, status history and payment.
/// Prices and addresses are snapshots. All transitions follow the table in docs/03-domain-model.md §5.
/// </summary>
public sealed class Order : AggregateRoot
{
    public const int MaxQuantityPerLine = 99;
    public const int MaxLines = 50;
    public const int MaxEmailLength = 256;
    public const int MaxNoteLength = 500;
    public const int MaxInternalNoteLength = 2000;
    public const int MaxIdempotencyKeyLength = 64;

    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

    private Order()
    {
    }

    private Order(Guid id, OrderNumber orderNumber, Guid? customerId, string contactEmail, AddressInfo shippingAddress,
        PaymentMethod paymentMethod, string? customerNote, string? idempotencyKey, DateTimeOffset placedAt)
        : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        ContactEmail = contactEmail;
        ShippingAddress = shippingAddress;
        PaymentMethod = paymentMethod;
        CustomerNote = customerNote;
        IdempotencyKey = idempotencyKey;
        PlacedAt = placedAt;
        LastChangedAt = placedAt;
        Status = OrderStatus.Pending;
    }

    public OrderNumber OrderNumber { get; private set; } = default!;

    /// <summary>Null for guest checkout.</summary>
    public Guid? CustomerId { get; private set; }

    public string ContactEmail { get; private set; } = default!;

    public AddressInfo ShippingAddress { get; private set; } = default!;

    public OrderStatus Status { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public Money Subtotal { get; private set; } = default!;

    public Money ShippingFee { get; private set; } = default!;

    public Money DiscountAmount { get; private set; } = default!;

    public string? CouponCode { get; private set; }

    public Money Total { get; private set; } = default!;

    public string? CustomerNote { get; private set; }

    public string? InternalNote { get; private set; }

    /// <summary>Client-supplied key making checkout retries safe (same key → same order).</summary>
    public string? IdempotencyKey { get; private set; }

    public DateTimeOffset PlacedAt { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public DateTimeOffset? ShippedAt { get; private set; }

    public DateTimeOffset? DeliveredAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public string? CancelReason { get; private set; }

    public ActorType? CancelledBy { get; private set; }

    public Payment Payment { get; private set; } = default!;

    /// <summary>
    /// Set by every change, including payment-only changes, so the order row (and its concurrency token) is always
    /// updated: two actors can never both change an order based on the same stale version.
    /// </summary>
    public DateTimeOffset LastChangedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public static Result<Order> Place(PlaceOrderData data, IReadOnlyList<OrderLine> lines, Money shippingFee, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(shippingFee);

        if (lines.Count == 0)
        {
            return Result.Failure<Order>(OrderErrors.EmptyOrder);
        }

        if (lines.Count > MaxLines)
        {
            return Result.Failure<Order>(OrderErrors.TooManyLines);
        }

        if (lines.Any(l => l.Quantity is < 1 or > MaxQuantityPerLine))
        {
            return Result.Failure<Order>(OrderErrors.InvalidQuantity);
        }

        var order = new Order(Guid.CreateVersion7(), data.OrderNumber, data.CustomerId, data.ContactEmail.Trim().ToLowerInvariant(),
            data.ShippingAddress.Normalize(), data.PaymentMethod, Clean(data.CustomerNote), Clean(data.IdempotencyKey), now);

        order._items.AddRange(lines.Select(l => new OrderItem(order.Id, l)));
        order.Subtotal = order._items.Aggregate(Money.Zero(shippingFee.Currency), (sum, i) => sum + i.LineTotal);
        order.ShippingFee = shippingFee;
        order.DiscountAmount = Money.Zero(shippingFee.Currency);
        order.Total = order.Subtotal + order.ShippingFee;

        var transferContent = data.PaymentMethod == PaymentMethod.BankTransfer ? data.OrderNumber.TransferContent : null;
        order.Payment = new Payment(order.Id, data.PaymentMethod, order.Total, transferContent);

        order._statusHistory.Add(new OrderStatusHistory(order.Id, null, OrderStatus.Pending, OrderActor.Customer(data.CustomerId), null, now));
        order.Raise(new OrderPlacedDomainEvent(order.Id, now));
        return order;
    }

    /// <summary>True while the customer may still cancel on their own (pending and not yet paid).</summary>
    public bool CanCustomerCancel => Status == OrderStatus.Pending && !Payment.IsPaid;

    /// <summary>Staff accepts the order. Bank transfers must be paid first, or be marked paid in the same step.</summary>
    public Result Confirm(OrderActor actor, DateTimeOffset now, bool markPaid, string? transactionRef)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        if (PaymentMethod == PaymentMethod.BankTransfer && !Payment.IsPaid)
        {
            if (!markPaid)
            {
                return Result.Failure(OrderErrors.PaymentRequired);
            }

            Payment.MarkPaid(transactionRef, now, actor.UserId);
        }

        ConfirmedAt = now;
        ChangeStatus(OrderStatus.Confirmed, actor, null, now);
        Raise(new OrderConfirmedDomainEvent(Id, now));
        return Result.Success();
    }

    public Result StartPreparing(OrderActor actor, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (Status != OrderStatus.Confirmed)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        ChangeStatus(OrderStatus.Preparing, actor, null, now);
        return Result.Success();
    }

    public Result Ship(OrderActor actor, string? note, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (Status != OrderStatus.Preparing)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        ShippedAt = now;
        ChangeStatus(OrderStatus.Shipping, actor, Clean(note), now);
        Raise(new OrderShippedDomainEvent(Id, now));
        return Result.Success();
    }

    /// <summary>Marks the order delivered. Cash-on-delivery payments become paid.</summary>
    public Result MarkDelivered(OrderActor actor, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (Status != OrderStatus.Shipping)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        if (PaymentMethod == PaymentMethod.Cod && !Payment.IsPaid)
        {
            Payment.MarkPaid(null, now, actor.UserId);
        }

        DeliveredAt = now;
        ChangeStatus(OrderStatus.Delivered, actor, null, now);
        Raise(new OrderDeliveredDomainEvent(Id, now));
        return Result.Success();
    }

    /// <summary>
    /// Cancels the order and tells the caller what to do with stock: pending orders release their reservation,
    /// confirmed/preparing orders return goods to stock, shipping orders need a manual stock adjustment.
    /// </summary>
    public Result<CancelStockEffect> Cancel(OrderActor actor, string reason, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return Result.Failure<CancelStockEffect>(OrderErrors.InvalidStatusTransition);
        }

        if (actor.Type != ActorType.Staff && Status != OrderStatus.Pending)
        {
            return Result.Failure<CancelStockEffect>(OrderErrors.CustomerCanOnlyCancelPending);
        }

        if (actor.Type != ActorType.Staff && Payment.IsPaid)
        {
            return Result.Failure<CancelStockEffect>(OrderErrors.PaidOrderNeedsStaff);
        }

        var effect = Status switch
        {
            OrderStatus.Pending => CancelStockEffect.ReleaseReservation,
            OrderStatus.Confirmed or OrderStatus.Preparing => CancelStockEffect.ReturnToStock,
            _ => CancelStockEffect.None,
        };

        if (Payment.IsPaid)
        {
            Payment.MarkRefundPending();
        }

        CancelledAt = now;
        CancelReason = reason.Trim();
        CancelledBy = actor.Type;
        ChangeStatus(OrderStatus.Cancelled, actor, CancelReason, now);
        Raise(new OrderCancelledDomainEvent(Id, CancelReason, now));
        return effect;
    }

    /// <summary>Records a received payment (bank transfer reconciliation) without changing the status.</summary>
    public Result MarkPaid(OrderActor actor, string? transactionRef, DateTimeOffset paidAt, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (Status == OrderStatus.Cancelled)
        {
            return Result.Failure(OrderErrors.CannotPayCancelledOrder);
        }

        if (Payment.IsPaid)
        {
            return Result.Failure(OrderErrors.AlreadyPaid);
        }

        Payment.MarkPaid(transactionRef, paidAt, actor.UserId);
        LastChangedAt = now;
        return Result.Success();
    }

    /// <summary>Staff confirms the money of a cancelled, paid order was returned to the customer.</summary>
    public Result MarkRefunded(OrderActor actor, string? transactionRef, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (Payment.Status != PaymentStatus.RefundPending)
        {
            return Result.Failure(OrderErrors.NoRefundPending);
        }

        Payment.MarkRefunded(transactionRef);
        LastChangedAt = now;
        return Result.Success();
    }

    public void SetInternalNote(string? note, DateTimeOffset now)
    {
        InternalNote = Clean(note);
        LastChangedAt = now;
    }

    private void ChangeStatus(OrderStatus to, OrderActor actor, string? note, DateTimeOffset now)
    {
        _statusHistory.Add(new OrderStatusHistory(Id, Status, to, actor, note, now));
        Status = to;
        LastChangedAt = now;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>Customer-provided checkout data (already validated by the application layer).</summary>
public sealed record PlaceOrderData(
    OrderNumber OrderNumber,
    Guid? CustomerId,
    string ContactEmail,
    AddressInfo ShippingAddress,
    PaymentMethod PaymentMethod,
    string? CustomerNote,
    string? IdempotencyKey);
