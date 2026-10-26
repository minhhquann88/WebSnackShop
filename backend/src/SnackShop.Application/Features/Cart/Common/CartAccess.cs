using SnackShop.Application.Features.Me.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Cart.Common;

/// <summary>Returns the signed-in customer's cart, creating the profile and the cart on first use.</summary>
public sealed class CartAccess(CustomerProfiles profiles, ICartRepository carts, TimeProvider timeProvider)
{
    public async Task<Result<Domain.Customers.Cart>> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var customer = await profiles.GetOrCreateAsync(cancellationToken);
        if (customer.IsFailure)
        {
            return Result.Failure<Domain.Customers.Cart>(customer.Error);
        }

        var cart = await carts.GetByCustomerIdAsync(customer.Value.Id, cancellationToken);
        if (cart is null)
        {
            cart = Domain.Customers.Cart.CreateFor(customer.Value.Id, timeProvider.GetUtcNow());
            carts.Add(cart);
        }

        return cart;
    }
}
