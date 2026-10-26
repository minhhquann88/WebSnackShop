using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Auth.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Auth.RefreshSession;

public sealed class RefreshSessionCommandHandler(
    IRefreshTokenService refreshTokens,
    IIdentityService identity,
    AuthSessionIssuer sessions) : ICommandHandler<RefreshSessionCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(RefreshSessionCommand command, CancellationToken cancellationToken)
    {
        var rotated = await refreshTokens.RotateAsync(command.RefreshToken, cancellationToken);
        if (rotated.IsFailure)
        {
            return Result.Failure<AuthResponse>(rotated.Error);
        }

        var account = await identity.FindByIdAsync(rotated.Value.UserId, cancellationToken);
        if (account is null || account.IsLocked)
        {
            // Failure: the unit of work does not commit, so the rotation above is discarded too.
            return Result.Failure<AuthResponse>(account is null ? AuthErrors.InvalidRefreshToken : AuthErrors.LockedOut);
        }

        return sessions.FromRotation(account, rotated.Value.NewToken);
    }
}
