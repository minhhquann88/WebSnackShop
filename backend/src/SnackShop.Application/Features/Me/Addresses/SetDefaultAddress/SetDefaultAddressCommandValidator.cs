using FluentValidation;

namespace SnackShop.Application.Features.Me.Addresses.SetDefaultAddress;

public sealed class SetDefaultAddressCommandValidator : AbstractValidator<SetDefaultAddressCommand>
{
    public SetDefaultAddressCommandValidator()
    {
        RuleFor(x => x.AddressId).NotEmpty();
    }
}
