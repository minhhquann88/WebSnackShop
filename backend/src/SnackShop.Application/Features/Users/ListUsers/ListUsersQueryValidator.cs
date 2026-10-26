using FluentValidation;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Users.ListUsers;

public sealed class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    public ListUsersQueryValidator()
    {
        this.ValidatePaging();
        RuleFor(x => x.Role).Must(r => r is null || Roles.All.Contains(r)).WithMessage("Unknown role.");
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
