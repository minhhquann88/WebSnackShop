using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Cart.MergeCart;

/// <summary>Merges a guest (browser) cart into the server cart after sign-in. Unavailable variants are skipped.</summary>
public sealed record MergeCartCommand(IReadOnlyList<CartLineRequest> Items) : ICommand;
