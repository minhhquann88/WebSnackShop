using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Features.Auth.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(IIdentityService identity, AuthSessionIssuer sessions)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var account = await identity.CheckPasswordAsync(command.Email.Trim(), command.Password, cancellationToken);
        if (account.IsFailure)
        {
            return Result.Failure<AuthResponse>(account.Error);
        }

        return await sessions.IssueAsync(account.Value, cancellationToken);
    }
}
