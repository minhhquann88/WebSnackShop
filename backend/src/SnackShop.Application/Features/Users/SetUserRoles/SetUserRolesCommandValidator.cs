using FluentValidation;
using SnackShop.Application.Common.Authorization;

namespace SnackShop.Application.Features.Users.SetUserRoles;

public sealed class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
        RuleForEach(x => x.Roles).Must(r => Roles.All.Contains(r)).WithMessage("Unknown role.");
    }
}
