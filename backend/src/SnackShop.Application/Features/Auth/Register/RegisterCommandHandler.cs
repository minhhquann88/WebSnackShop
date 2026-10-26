using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Auth.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler(
    IIdentityService identity,
    ICustomerRepository customers,
    AuthSessionIssuer sessions,
    AccountEmails accountEmails) : ICommandHandler<RegisterCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var userId = Guid.CreateVersion7();
        var email = command.Email.Trim().ToLowerInvariant();

        // Stage the profile first: Identity saves the shared DbContext, so user and customer commit together.
        customers.Add(Customer.Create(userId, command.FullName, email, command.PhoneNumber));

        var created = await identity.CreateUserAsync(
            new NewUserAccount(userId, email, command.Password, command.FullName.Trim(), command.PhoneNumber?.Trim(),
                AvatarUrl: null, EmailConfirmed: false, [Roles.Customer]),
            cancellationToken);

        if (created.IsFailure)
        {
            return Result.Failure<AuthResponse>(created.Error);
        }

        await accountEmails.SendConfirmationAsync(created.Value, cancellationToken);
        return await sessions.IssueAsync(created.Value, cancellationToken);
    }
}
