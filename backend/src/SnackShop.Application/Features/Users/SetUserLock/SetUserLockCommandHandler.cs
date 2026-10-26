using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Users.SetUserLock;

public sealed class SetUserLockCommandHandler(IIdentityService identity, ICurrentUser currentUser, IRefreshTokenService refreshTokens)
    : ICommandHandler<SetUserLockCommand>
{
    public async Task<Result> Handle(SetUserLockCommand command, CancellationToken cancellationToken)
    {
        if (command.Locked && command.UserId == currentUser.UserId)
        {
            return Result.Failure(AuthErrors.CannotChangeOwnRoles);
        }

        var updated = await identity.SetLockedAsync(command.UserId, command.Locked, cancellationToken);
        if (updated.IsFailure)
        {
            return updated;
        }

        if (command.Locked)
        {
            await refreshTokens.RevokeAllForUserAsync(command.UserId, "Locked", cancellationToken);
        }

        return Result.Success();
    }
}
