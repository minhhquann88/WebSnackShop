using FluentValidation;

namespace SnackShop.Application.Features.Me.Addresses.DeleteMyAddress;

public sealed class DeleteMyAddressCommandValidator : AbstractValidator<DeleteMyAddressCommand>
{
    public DeleteMyAddressCommandValidator()
    {
        RuleFor(x => x.AddressId).NotEmpty();
    }
}
