using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Inventory.AdjustStock;

/// <summary>Manual correction after a stock count, damage or loss. <see cref="Delta"/> may be negative.</summary>
public sealed record AdjustStockCommand(Guid VariantId, int Delta, string Reason) : ICommand;
