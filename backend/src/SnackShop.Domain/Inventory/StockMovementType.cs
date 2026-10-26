namespace SnackShop.Domain.Inventory;

public enum StockMovementType
{
    /// <summary>Goods received from a supplier. OnHand +.</summary>
    Purchase = 0,

    /// <summary>Reserved quantity committed when an order is confirmed. OnHand −, Reserved −.</summary>
    Sale = 1,

    /// <summary>Manual correction by an admin (stock count, damage). OnHand ±.</summary>
    Adjustment = 2,

    /// <summary>Goods put back after a confirmed order is cancelled. OnHand +.</summary>
    Return = 3,

    /// <summary>Quantity held for a pending order. Reserved +.</summary>
    Reservation = 4,

    /// <summary>Hold released because a pending order was cancelled. Reserved −.</summary>
    ReservationRelease = 5,
}
