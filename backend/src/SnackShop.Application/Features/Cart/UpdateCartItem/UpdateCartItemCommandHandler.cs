using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Cart.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Cart.UpdateCartItem;

public sealed class UpdateCartItemCommandHandler(CartAccess cartAccess, TimeProvider timeProvider)
    : ICommandHandler<UpdateCartItemCommand>
{
    public async Task<Result> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartAccess.GetOrCreateAsync(cancellationToken);
        return cart.IsFailure ? cart : cart.Value.SetQuantity(command.VariantId, command.Quantity, timeProvider.GetUtcNow());
    }
}
