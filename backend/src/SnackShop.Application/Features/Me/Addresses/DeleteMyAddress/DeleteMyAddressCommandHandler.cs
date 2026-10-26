using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Me.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Me.Addresses.DeleteMyAddress;

public sealed class DeleteMyAddressCommandHandler(CustomerProfiles profiles) : ICommandHandler<DeleteMyAddressCommand>
{
    public async Task<Result> Handle(DeleteMyAddressCommand command, CancellationToken cancellationToken)
    {
        var customer = await profiles.GetOrCreateAsync(cancellationToken);
        return customer.IsFailure ? customer : customer.Value.RemoveAddress(command.AddressId);
    }
}
