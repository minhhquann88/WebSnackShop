using FluentValidation;
using SnackShop.Application.Common.Validation;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Auth.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddressRule();
        RuleFor(x => x.Password).Password();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(Customer.MaxNameLength);
        RuleFor(x => x.PhoneNumber!).VietnamesePhone().When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
