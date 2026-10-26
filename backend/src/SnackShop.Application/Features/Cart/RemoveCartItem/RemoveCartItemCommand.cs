using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Cart.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid VariantId) : ICommand;
