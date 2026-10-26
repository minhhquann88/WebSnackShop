using FluentValidation;
using SnackShop.Application.Common.Validation;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Users.CreateStaff;

public sealed class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddressRule();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(Customer.MaxNameLength);
        RuleFor(x => x.Password).Password();
    }
}
