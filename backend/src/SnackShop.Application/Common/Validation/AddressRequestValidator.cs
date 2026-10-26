using FluentValidation;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Common.Validation;

public sealed class AddressRequestValidator : AbstractValidator<AddressRequest>
{
    public AddressRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(AddressInfo.MaxNameLength);
        RuleFor(x => x.Phone).VietnamesePhone();
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(AddressInfo.MaxLineLength);
        RuleFor(x => x.Ward).NotEmpty().MaximumLength(AddressInfo.MaxAreaLength);
        RuleFor(x => x.District).NotEmpty().MaximumLength(AddressInfo.MaxAreaLength);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(AddressInfo.MaxAreaLength);
    }
}
