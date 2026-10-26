using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler(IIdentityService identity) : ICommandHandler<ConfirmEmailCommand>
{
    public Task<Result> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken) =>
        identity.ConfirmEmailAsync(command.UserId, command.Token, cancellationToken);
}
