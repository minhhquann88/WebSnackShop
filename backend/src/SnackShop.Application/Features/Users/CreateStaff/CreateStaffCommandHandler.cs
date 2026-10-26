using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common.Authorization;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Users.CreateStaff;

public sealed class CreateStaffCommandHandler(IIdentityService identity) : ICommandHandler<CreateStaffCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateStaffCommand command, CancellationToken cancellationToken)
    {
        string[] roles = command.IsAdmin ? [Roles.Staff, Roles.Admin] : [Roles.Staff];
        var created = await identity.CreateUserAsync(
            new NewUserAccount(Guid.CreateVersion7(), command.Email.Trim().ToLowerInvariant(), command.Password,
                command.FullName.Trim(), PhoneNumber: null, AvatarUrl: null, EmailConfirmed: true, roles),
            cancellationToken);

        return created.IsFailure ? Result.Failure<Guid>(created.Error) : created.Value.Id;
    }
}
