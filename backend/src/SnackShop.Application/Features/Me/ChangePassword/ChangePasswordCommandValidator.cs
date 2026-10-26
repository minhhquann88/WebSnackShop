using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Me.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).MaximumLength(ValidationRules.MaxPasswordLength);
        RuleFor(x => x.NewPassword).Password().NotEqual(x => x.CurrentPassword)
            .WithMessage("The new password must differ from the current one.");
    }
}
