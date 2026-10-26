using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.DeleteProductImage;

public sealed record DeleteProductImageCommand(Guid ProductId, Guid ImageId) : ICommand;
