using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddressRule();
    }
}
