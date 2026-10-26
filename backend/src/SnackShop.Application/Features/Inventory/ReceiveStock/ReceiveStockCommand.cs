using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Inventory.ReceiveStock;

/// <summary>Records goods received from a supplier.</summary>
public sealed record ReceiveStockCommand(Guid VariantId, int Quantity, string? Note) : ICommand;
