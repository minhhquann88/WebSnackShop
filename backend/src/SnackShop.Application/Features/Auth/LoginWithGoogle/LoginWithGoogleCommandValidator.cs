using FluentValidation;

namespace SnackShop.Application.Features.Auth.LoginWithGoogle;

public sealed class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().MaximumLength(4096);
    }
}
