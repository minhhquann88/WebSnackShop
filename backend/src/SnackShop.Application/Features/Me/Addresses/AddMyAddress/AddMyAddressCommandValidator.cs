using FluentValidation;
using SnackShop.Application.Common.Validation;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Me.Addresses.AddMyAddress;

public sealed class AddMyAddressCommandValidator : AbstractValidator<AddMyAddressCommand>
{
    public AddMyAddressCommandValidator()
    {
        RuleFor(x => x.Label).MaximumLength(Address.MaxLabelLength);
        RuleFor(x => x.Address).NotNull().SetValidator(new AddressRequestValidator());
    }
}
