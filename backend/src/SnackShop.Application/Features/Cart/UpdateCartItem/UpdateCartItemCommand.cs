using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Cart.UpdateCartItem;

/// <summary>Sets the quantity of a variant already in the cart.</summary>
public sealed record UpdateCartItemCommand(Guid VariantId, int Quantity) : ICommand;
