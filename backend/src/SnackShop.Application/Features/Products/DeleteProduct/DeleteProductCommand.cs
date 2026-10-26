using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.DeleteProduct;

/// <summary>Soft-deletes a product. Past orders keep their snapshots.</summary>
public sealed record DeleteProductCommand(Guid Id) : ICommand;
