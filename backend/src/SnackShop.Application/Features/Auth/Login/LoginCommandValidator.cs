using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(ValidationRules.MaxEmailLength);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(ValidationRules.MaxPasswordLength);
    }
}
