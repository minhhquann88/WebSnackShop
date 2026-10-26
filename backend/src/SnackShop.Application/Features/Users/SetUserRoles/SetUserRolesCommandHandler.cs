using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Authorization;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Users.SetUserRoles;

public sealed class SetUserRolesCommandHandler(IIdentityService identity, ICurrentUser currentUser, IRefreshTokenService refreshTokens)
    : ICommandHandler<SetUserRolesCommand>
{
    public async Task<Result> Handle(SetUserRolesCommand command, CancellationToken cancellationToken)
    {
        if (command.UserId == currentUser.UserId && !command.Roles.Contains(Roles.Admin))
        {
            return Result.Failure(AuthErrors.CannotChangeOwnRoles);
        }

        // Admin implies Staff so policies stay simple.
        var roles = command.Roles.Contains(Roles.Admin) ? command.Roles.Append(Roles.Staff).Distinct().ToList() : command.Roles.Distinct().ToList();
        var updated = await identity.SetRolesAsync(command.UserId, roles, cancellationToken);
        if (updated.IsFailure)
        {
            return updated;
        }

        // Force a new sign-in so the access token carries the new roles.
        await refreshTokens.RevokeAllForUserAsync(command.UserId, "RolesChanged", cancellationToken);
        return Result.Success();
    }
}
