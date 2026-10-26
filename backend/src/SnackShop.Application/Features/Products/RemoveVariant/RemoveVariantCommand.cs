using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.RemoveVariant;

/// <summary>Removes a variant from a product. Its stock record and history are kept for audit.</summary>
public sealed record RemoveVariantCommand(Guid ProductId, Guid VariantId) : ICommand;
