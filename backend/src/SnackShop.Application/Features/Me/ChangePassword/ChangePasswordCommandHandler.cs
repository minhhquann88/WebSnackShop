using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Me.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    ICurrentUser currentUser,
    IIdentityService identity,
    IRefreshTokenService refreshTokens) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return userId;
        }

        var changed = await identity.ChangePasswordAsync(userId.Value, command.CurrentPassword ?? string.Empty, command.NewPassword, cancellationToken);
        if (changed.IsFailure)
        {
            return changed;
        }

        await refreshTokens.RevokeAllForUserAsync(userId.Value, "PasswordChanged", cancellationToken);
        return Result.Success();
    }
}
