using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common.Catalog;
using SnackShop.Application.Features.Cart.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Cart.AddCartItem;

public sealed class AddCartItemCommandHandler(CartAccess cartAccess, PurchasableVariants variants, TimeProvider timeProvider)
    : ICommandHandler<AddCartItemCommand>
{
    public async Task<Result> Handle(AddCartItemCommand command, CancellationToken cancellationToken)
    {
        var snapshots = await variants.LoadAsync([command.VariantId], cancellationToken);
        if (!snapshots.TryGetValue(command.VariantId, out var variant) || !variant.IsPurchasable)
        {
            return Result.Failure(CatalogErrors.VariantNotFound);
        }

        var cart = await cartAccess.GetOrCreateAsync(cancellationToken);
        return cart.IsFailure ? cart : cart.Value.AddItem(command.VariantId, command.Quantity, timeProvider.GetUtcNow());
    }
}
