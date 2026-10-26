using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.SetPrimaryImage;

public sealed record SetPrimaryImageCommand(Guid ProductId, Guid ImageId) : ICommand;
