using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Me.Common;

/// <summary>
/// Loads the shopping profile of the signed-in user, creating it on first use. Registered customers get one at
/// sign-up; staff accounts get one lazily the first time they use the cart or address book.
/// </summary>
public sealed class CustomerProfiles(ICurrentUser currentUser, ICustomerRepository customers, IIdentityService identity)
{
    public async Task<Result<Customer>> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return Result.Failure<Customer>(userId.Error);
        }

        var customer = await customers.GetByIdAsync(userId.Value, cancellationToken);
        if (customer is not null)
        {
            return customer;
        }

        var account = await identity.FindByIdAsync(userId.Value, cancellationToken);
        if (account is null)
        {
            return Result.Failure<Customer>(AuthErrors.UserNotFound);
        }

        customer = Customer.Create(account.Id, account.FullName, account.Email, account.PhoneNumber);
        customers.Add(customer);
        return customer;
    }
}
