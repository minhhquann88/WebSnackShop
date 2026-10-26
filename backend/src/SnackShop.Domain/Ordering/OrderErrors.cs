using SnackShop.Domain.Common;

namespace SnackShop.Domain.Ordering;

public static class OrderErrors
{
    public static readonly Error NotFound = Error.NotFound("Orders.NotFound", "The order was not found.");

    public static readonly Error Forbidden = Error.Forbidden("Orders.Forbidden", "You do not have access to this order.");

    public static readonly Error EmptyOrder = Error.Validation("Orders.Empty", "An order must contain at least one item.");

    public static readonly Error InvalidQuantity = Error.Validation(
        "Orders.InvalidQuantity", $"Each item quantity must be between 1 and {Order.MaxQuantityPerLine}.");

    public static readonly Error TooManyLines = Error.Validation(
        "Orders.TooManyLines", $"An order can contain at most {Order.MaxLines} different items.");

    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "Orders.InvalidStatusTransition", "The order cannot move to the requested status.");

    public static readonly Error CustomerCanOnlyCancelPending = Error.Conflict(
        "Orders.CustomerCanOnlyCancelPending", "Orders can only be cancelled by the customer while they are pending.");

    public static readonly Error PaymentRequired = Error.Conflict(
        "Orders.PaymentRequired", "A bank transfer order must be marked as paid before it can be confirmed.");

    public static readonly Error AlreadyPaid = Error.Conflict("Orders.AlreadyPaid", "The order has already been paid.");

    public static readonly Error CannotPayCancelledOrder = Error.Conflict("Orders.CannotPayCancelledOrder", "A cancelled order cannot be marked as paid.");

    public static readonly Error PaidOrderNeedsStaff = Error.Conflict(
        "Orders.PaidOrderNeedsStaff", "This order has already been paid. Please contact the shop to cancel it and get a refund.");

    public static readonly Error NoRefundPending = Error.Conflict("Orders.NoRefundPending", "There is no pending refund on this order.");

    public static readonly Error IdempotencyKeyReused = Error.Conflict(
        "Orders.IdempotencyKeyReused", "This Idempotency-Key was already used for a different order. Generate a new key.");

    public static readonly Error ProductUnavailable = Error.Conflict(
        "Orders.ProductUnavailable", "One or more products in the order are no longer available.");
}
