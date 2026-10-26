using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Me.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Me.Addresses.UpdateMyAddress;

public sealed class UpdateMyAddressCommandHandler(CustomerProfiles profiles) : ICommandHandler<UpdateMyAddressCommand>
{
    public async Task<Result> Handle(UpdateMyAddressCommand command, CancellationToken cancellationToken)
    {
        var customer = await profiles.GetOrCreateAsync(cancellationToken);
        return customer.IsFailure
            ? customer
            : customer.Value.UpdateAddress(command.AddressId, command.Label, command.Address.ToInfo());
    }
}
