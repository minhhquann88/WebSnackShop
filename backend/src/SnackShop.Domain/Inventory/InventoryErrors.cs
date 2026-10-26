using SnackShop.Domain.Common;

namespace SnackShop.Domain.Inventory;

public static class InventoryErrors
{
    public static readonly Error StockItemNotFound = Error.NotFound("Inventory.StockItemNotFound", "No stock record exists for this variant.");

    public static readonly Error InvalidQuantity = Error.Validation("Inventory.InvalidQuantity", "Quantity must be greater than zero.");

    public static readonly Error InvalidThreshold = Error.Validation("Inventory.InvalidThreshold", "Low stock threshold cannot be negative.");

    public static readonly Error AdjustmentBelowReserved = Error.Conflict(
        "Inventory.AdjustmentBelowReserved",
        "The adjustment would leave less stock on hand than is reserved for pending orders.");

    public static readonly Error ReservationMismatch = Error.Conflict(
        "Inventory.ReservationMismatch", "Cannot release or commit more than is currently reserved.");

    public static Error InsufficientStock(string sku, int available) => Error.Conflict(
        "Inventory.InsufficientStock", $"Only {available} unit(s) of {sku} are available.");
}
