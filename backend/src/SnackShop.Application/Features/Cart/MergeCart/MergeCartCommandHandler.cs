using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common.Catalog;
using SnackShop.Application.Features.Cart.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Cart.MergeCart;

public sealed class MergeCartCommandHandler(CartAccess cartAccess, PurchasableVariants variants, TimeProvider timeProvider)
    : ICommandHandler<MergeCartCommand>
{
    public async Task<Result> Handle(MergeCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartAccess.GetOrCreateAsync(cancellationToken);
        if (cart.IsFailure)
        {
            return cart;
        }

        var snapshots = await variants.LoadAsync(command.Items.Select(i => i.VariantId).ToList(), cancellationToken);
        var valid = command.Items
            .Where(i => snapshots.TryGetValue(i.VariantId, out var v) && v.IsPurchasable)
            .Select(i => (i.VariantId, i.Quantity));

        cart.Value.Merge(valid, timeProvider.GetUtcNow());
        return Result.Success();
    }
}
