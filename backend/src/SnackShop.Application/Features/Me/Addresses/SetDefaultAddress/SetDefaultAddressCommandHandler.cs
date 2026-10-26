using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Me.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Me.Addresses.SetDefaultAddress;

public sealed class SetDefaultAddressCommandHandler(CustomerProfiles profiles) : ICommandHandler<SetDefaultAddressCommand>
{
    public async Task<Result> Handle(SetDefaultAddressCommand command, CancellationToken cancellationToken)
    {
        var customer = await profiles.GetOrCreateAsync(cancellationToken);
        return customer.IsFailure ? customer : customer.Value.SetDefaultAddress(command.AddressId);
    }
}
