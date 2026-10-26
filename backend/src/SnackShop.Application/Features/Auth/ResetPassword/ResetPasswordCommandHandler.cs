using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler(IIdentityService identity, IRefreshTokenService refreshTokens)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var reset = await identity.ResetPasswordAsync(email, command.Token, command.NewPassword, cancellationToken);
        if (reset.IsFailure)
        {
            return reset;
        }

        var account = await identity.FindByEmailAsync(email, cancellationToken);
        if (account is not null)
        {
            await refreshTokens.RevokeAllForUserAsync(account.Id, "PasswordReset", cancellationToken);
        }

        return Result.Success();
    }
}
