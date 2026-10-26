using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Inventory;

public sealed record StockLevelResponse(
    Guid VariantId,
    Guid ProductId,
    string ProductName,
    string VariantName,
    string Sku,
    int QuantityOnHand,
    int QuantityReserved,
    int Available,
    int LowStockThreshold,
    bool IsLowStock);

public sealed record StockMovementResponse(
    Guid Id,
    StockMovementType Type,
    int Quantity,
    int OnHandAfter,
    int ReservedAfter,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Note,
    DateTimeOffset CreatedAt,
    Guid? CreatedBy);
