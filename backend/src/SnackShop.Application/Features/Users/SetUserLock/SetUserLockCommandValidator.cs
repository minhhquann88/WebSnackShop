using FluentValidation;

namespace SnackShop.Application.Features.Users.SetUserLock;

public sealed class SetUserLockCommandValidator : AbstractValidator<SetUserLockCommand>
{
    public SetUserLockCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
