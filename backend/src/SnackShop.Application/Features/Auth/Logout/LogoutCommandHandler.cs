using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler(IRefreshTokenService refreshTokens) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        await refreshTokens.RevokeAsync(command.RefreshToken, cancellationToken);
        return Result.Success();
    }
}
