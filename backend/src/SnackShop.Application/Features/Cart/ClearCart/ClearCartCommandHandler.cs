using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Cart.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Cart.ClearCart;

public sealed class ClearCartCommandHandler(CartAccess cartAccess, TimeProvider timeProvider) : ICommandHandler<ClearCartCommand>
{
    public async Task<Result> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await cartAccess.GetOrCreateAsync(cancellationToken);
        if (cart.IsFailure)
        {
            return cart;
        }

        cart.Value.Clear(timeProvider.GetUtcNow());
        return Result.Success();
    }
}
