using SnackShop.Domain.Common;

namespace SnackShop.Domain.Inventory;

/// <summary>
/// Stock of one product variant (Id == VariantId). Available = OnHand − Reserved.
/// Every change appends a <see cref="StockMovement"/>. Protected by optimistic concurrency (xmin).
/// See docs/adr/0008 for the reserve → commit / release policy.
/// </summary>
public sealed class StockItem : AggregateRoot
{
    public const int DefaultLowStockThreshold = 10;

    private readonly List<StockMovement> _movements = [];

    private StockItem()
    {
    }

    private StockItem(Guid variantId, int lowStockThreshold)
        : base(variantId)
    {
        LowStockThreshold = lowStockThreshold;
    }

    public Guid VariantId => Id;

    public int QuantityOnHand { get; private set; }

    public int QuantityReserved { get; private set; }

    public int LowStockThreshold { get; private set; }

    public int Available => QuantityOnHand - QuantityReserved;

    public bool IsLowStock => Available <= LowStockThreshold;

    /// <summary>New movements recorded on this instance (history is not loaded with the item).</summary>
    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    public static StockItem CreateFor(Guid variantId, int lowStockThreshold = DefaultLowStockThreshold) =>
        new(variantId, lowStockThreshold);

    public Result Receive(int quantity, string? note, StockContext context)
    {
        if (quantity <= 0)
        {
            return Result.Failure(InventoryErrors.InvalidQuantity);
        }

        QuantityOnHand += quantity;
        Record(StockMovementType.Purchase, quantity, null, null, note, context);
        return Result.Success();
    }

    /// <summary>Manual correction. <paramref name="delta"/> may be negative but OnHand cannot drop below Reserved.</summary>
    public Result Adjust(int delta, string reason, StockContext context)
    {
        if (delta == 0)
        {
            return Result.Failure(InventoryErrors.InvalidQuantity);
        }

        if (QuantityOnHand + delta < QuantityReserved)
        {
            return Result.Failure(InventoryErrors.AdjustmentBelowReserved);
        }

        QuantityOnHand += delta;
        Record(StockMovementType.Adjustment, delta, null, null, reason, context);
        return Result.Success();
    }

    public Result SetLowStockThreshold(int threshold)
    {
        if (threshold < 0)
        {
            return Result.Failure(InventoryErrors.InvalidThreshold);
        }

        LowStockThreshold = threshold;
        return Result.Success();
    }

    public Result Reserve(int quantity, string sku, Guid orderId, StockContext context)
    {
        if (quantity <= 0)
        {
            return Result.Failure(InventoryErrors.InvalidQuantity);
        }

        if (quantity > Available)
        {
            return Result.Failure(InventoryErrors.InsufficientStock(sku, Math.Max(0, Available)));
        }

        QuantityReserved += quantity;
        Record(StockMovementType.Reservation, quantity, StockMovement.OrderReference, orderId, null, context);
        return Result.Success();
    }

    public Result ReleaseReservation(int quantity, Guid orderId, StockContext context)
    {
        if (quantity <= 0 || quantity > QuantityReserved)
        {
            return Result.Failure(InventoryErrors.ReservationMismatch);
        }

        QuantityReserved -= quantity;
        Record(StockMovementType.ReservationRelease, -quantity, StockMovement.OrderReference, orderId, null, context);
        return Result.Success();
    }

    public Result CommitReservation(int quantity, Guid orderId, StockContext context)
    {
        if (quantity <= 0 || quantity > QuantityReserved || quantity > QuantityOnHand)
        {
            return Result.Failure(InventoryErrors.ReservationMismatch);
        }

        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
        Record(StockMovementType.Sale, -quantity, StockMovement.OrderReference, orderId, null, context);
        return Result.Success();
    }

    public Result Return(int quantity, Guid orderId, StockContext context)
    {
        if (quantity <= 0)
        {
            return Result.Failure(InventoryErrors.InvalidQuantity);
        }

        QuantityOnHand += quantity;
        Record(StockMovementType.Return, quantity, StockMovement.OrderReference, orderId, null, context);
        return Result.Success();
    }

    private void Record(StockMovementType type, int quantity, string? referenceType, Guid? referenceId, string? note, StockContext context)
    {
        var cleanNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        _movements.Add(new StockMovement(
            Id, type, quantity, QuantityOnHand, QuantityReserved, referenceType, referenceId, cleanNote, context.Now, context.UserId));
    }
}

/// <summary>Who and when, passed into stock operations so the domain never reads the clock or the user itself.</summary>
public sealed record StockContext(DateTimeOffset Now, Guid? UserId);
