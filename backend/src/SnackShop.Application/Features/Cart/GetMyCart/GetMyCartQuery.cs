using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Cart.GetMyCart;

/// <summary>The signed-in customer's cart priced with current prices and stock. Empty when none exists yet.</summary>
public sealed record GetMyCartQuery : IQuery<CartResponse>;
