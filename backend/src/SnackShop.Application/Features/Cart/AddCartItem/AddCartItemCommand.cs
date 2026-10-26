using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Cart.AddCartItem;

/// <summary>Adds a quantity of a variant to the cart (summed with what is already there).</summary>
public sealed record AddCartItemCommand(Guid VariantId, int Quantity) : ICommand;
