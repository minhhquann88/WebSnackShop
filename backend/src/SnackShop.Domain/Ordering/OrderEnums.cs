namespace SnackShop.Domain.Ordering;

/// <summary>Order lifecycle. Transitions are enforced by <see cref="Order"/> (docs/03 §5).</summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Preparing = 2,
    Shipping = 3,
    Delivered = 4,
    Cancelled = 5,
}

public enum PaymentMethod
{
    /// <summary>Cash on delivery.</summary>
    Cod = 0,

    /// <summary>Bank transfer (VietQR); confirmed manually by staff.</summary>
    BankTransfer = 1,
}

public enum PaymentStatus
{
    Unpaid = 0,
    Paid = 1,
    Refunded = 2,
    Failed = 3,

    /// <summary>A paid order was cancelled; staff must return the money and then mark it refunded.</summary>
    RefundPending = 4,
}

/// <summary>Who performs an order action. Determines which transitions are allowed.</summary>
public enum ActorType
{
    Customer = 0,
    Staff = 1,
    System = 2,
}

/// <summary>What the caller must do to inventory after an order is cancelled.</summary>
public enum CancelStockEffect
{
    None = 0,
    ReleaseReservation = 1,
    ReturnToStock = 2,
}
