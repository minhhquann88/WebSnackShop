using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Cart.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Cart.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler(CartAccess cartAccess, TimeProvider timeProvider)
    : ICommandHandler<RemoveCartItemCommand>
{
    public async Task<Result> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartAccess.GetOrCreateAsync(cancellationToken);
        return cart.IsFailure ? cart : cart.Value.RemoveItem(command.VariantId, timeProvider.GetUtcNow());
    }
}
