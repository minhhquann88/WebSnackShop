using FluentValidation;
using SnackShop.Application.Common.Validation;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Me.Addresses.UpdateMyAddress;

public sealed class UpdateMyAddressCommandValidator : AbstractValidator<UpdateMyAddressCommand>
{
    public UpdateMyAddressCommandValidator()
    {
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.Label).MaximumLength(Address.MaxLabelLength);
        RuleFor(x => x.Address).NotNull().SetValidator(new AddressRequestValidator());
    }
}
