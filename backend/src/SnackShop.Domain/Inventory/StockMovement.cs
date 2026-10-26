using SnackShop.Domain.Common;

namespace SnackShop.Domain.Inventory;

/// <summary>Append-only history line for a stock item. Never updated or deleted.</summary>
public sealed class StockMovement : Entity
{
    public const int MaxNoteLength = 500;
    public const string OrderReference = "Order";

    private StockMovement()
    {
    }

    internal StockMovement(
        Guid variantId,
        StockMovementType type,
        int quantity,
        int onHandAfter,
        int reservedAfter,
        string? referenceType,
        Guid? referenceId,
        string? note,
        DateTimeOffset createdAt,
        Guid? createdBy)
        : base(Guid.CreateVersion7())
    {
        VariantId = variantId;
        Type = type;
        Quantity = quantity;
        OnHandAfter = onHandAfter;
        ReservedAfter = reservedAfter;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Note = note;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid VariantId { get; private set; }

    public StockMovementType Type { get; private set; }

    /// <summary>Signed change of the affected counter (positive adds, negative removes).</summary>
    public int Quantity { get; private set; }

    public int OnHandAfter { get; private set; }

    public int ReservedAfter { get; private set; }

    public string? ReferenceType { get; private set; }

    public Guid? ReferenceId { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? CreatedBy { get; private set; }
}
