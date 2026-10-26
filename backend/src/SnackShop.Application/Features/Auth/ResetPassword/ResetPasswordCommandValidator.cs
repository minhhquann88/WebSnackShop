using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddressRule();
        RuleFor(x => x.Token).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.NewPassword).Password();
    }
}
