using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Inventory.SetLowStockThreshold;

public sealed record SetLowStockThresholdCommand(Guid VariantId, int Threshold) : ICommand;
