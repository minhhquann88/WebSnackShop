using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Me.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Me.Addresses.AddMyAddress;

public sealed class AddMyAddressCommandHandler(CustomerProfiles profiles) : ICommandHandler<AddMyAddressCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddMyAddressCommand command, CancellationToken cancellationToken)
    {
        var customer = await profiles.GetOrCreateAsync(cancellationToken);
        if (customer.IsFailure)
        {
            return Result.Failure<Guid>(customer.Error);
        }

        var added = customer.Value.AddAddress(command.Label, command.Address.ToInfo(), command.IsDefault);
        return added.IsFailure ? Result.Failure<Guid>(added.Error) : added.Value.Id;
    }
}
