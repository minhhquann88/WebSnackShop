using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Features.Me.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Me.UpdateProfile;

public sealed class UpdateProfileCommandHandler(CustomerProfiles profiles, IIdentityService identity)
    : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var customer = await profiles.GetOrCreateAsync(cancellationToken);
        if (customer.IsFailure)
        {
            return customer;
        }

        // Update the staged profile first; Identity's save then commits both together.
        customer.Value.UpdateProfile(command.FullName, command.PhoneNumber);
        return await identity.UpdateProfileAsync(customer.Value.Id, command.FullName, command.PhoneNumber, cancellationToken);
    }
}
