using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Features.Auth.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(IIdentityService identity, AccountEmails accountEmails)
    : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var token = await identity.GeneratePasswordResetTokenAsync(email, cancellationToken);

        if (token is not null)
        {
            await accountEmails.SendPasswordResetAsync(email, token, cancellationToken);
        }

        return Result.Success();
    }
}
