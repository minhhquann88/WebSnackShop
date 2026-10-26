using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Cart.GetMyCart;

public sealed class GetMyCartQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, PurchasableVariants variants)
    : IQueryHandler<GetMyCartQuery, CartResponse>
{
    public async Task<Result<CartResponse>> Handle(GetMyCartQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return Result.Failure<CartResponse>(userId.Error);
        }

        var cart = await db.Carts.AsNoTracking()
            .Where(c => c.CustomerId == userId.Value)
            .Select(c => new { c.UpdatedAt, Items = c.Items.OrderBy(i => i.AddedAt).Select(i => new { i.VariantId, i.Quantity }).ToList() })
            .FirstOrDefaultAsync(cancellationToken);

        if (cart is null || cart.Items.Count == 0)
        {
            return new CartResponse([], 0, MoneyResponse.From(Money.Zero()), cart?.UpdatedAt);
        }

        var snapshots = await variants.LoadAsync(cart.Items.Select(i => i.VariantId).ToList(), cancellationToken);

        // Variants deleted from the catalog silently drop out of the cart view.
        var lines = cart.Items
            .Where(i => snapshots.ContainsKey(i.VariantId))
            .Select(i => PricedLineResponse.From(snapshots[i.VariantId], i.Quantity))
            .ToList();

        var subtotal = lines.Where(l => l.IsAvailable).Sum(l => l.LineTotal.Amount);
        return new CartResponse(lines, lines.Sum(l => l.Quantity), new MoneyResponse(subtotal, Money.DefaultCurrency), cart.UpdatedAt);
    }
}
