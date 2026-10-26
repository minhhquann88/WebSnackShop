using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Auth.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Auth.LoginWithGoogle;

public sealed class LoginWithGoogleCommandHandler(
    IGoogleTokenVerifier google,
    IIdentityService identity,
    ICustomerRepository customers,
    AuthSessionIssuer sessions) : ICommandHandler<LoginWithGoogleCommand, AuthResponse>
{
    public const string Provider = "Google";

    public static readonly Error StaffMustUsePassword = Error.Forbidden(
        "Auth.StaffMustUsePassword", "Staff and admin accounts must sign in with email and password.");

    /// <summary>
    /// An unconfirmed password account with the same email could have been registered by someone else
    /// (account pre-hijacking), so it is never linked automatically.
    /// </summary>
    public static readonly Error UnconfirmedAccountExists = Error.Conflict(
        "Auth.UnconfirmedAccountExists",
        "An account with this email exists but its email is not confirmed. Sign in with the password, or confirm the email first.");

    public async Task<Result<AuthResponse>> Handle(LoginWithGoogleCommand command, CancellationToken cancellationToken)
    {
        var verified = await google.VerifyAsync(command.IdToken, cancellationToken);
        if (verified.IsFailure)
        {
            return Result.Failure<AuthResponse>(verified.Error);
        }

        var googleUser = verified.Value;
        if (!googleUser.EmailVerified)
        {
            return Result.Failure<AuthResponse>(AuthErrors.GoogleEmailNotVerified);
        }

        var linkedAccount = await identity.FindByExternalLoginAsync(Provider, googleUser.Subject, cancellationToken);
        var account = linkedAccount is not null
            ? CheckAllowed(linkedAccount)
            : await LinkOrCreateAsync(googleUser, cancellationToken);

        return account.IsFailure
            ? Result.Failure<AuthResponse>(account.Error)
            : await sessions.IssueAsync(account.Value, cancellationToken);
    }

    private async Task<Result<UserAccount>> LinkOrCreateAsync(GoogleUser googleUser, CancellationToken cancellationToken)
    {
        var email = googleUser.Email.Trim().ToLowerInvariant();
        var existing = await identity.FindByEmailAsync(email, cancellationToken);

        if (existing is not null)
        {
            // Check before linking: Identity persists the link immediately.
            var allowed = CheckAllowed(existing);
            if (allowed.IsFailure)
            {
                return allowed;
            }

            if (!existing.EmailConfirmed)
            {
                return Result.Failure<UserAccount>(UnconfirmedAccountExists);
            }

            var linked = await identity.AddExternalLoginAsync(existing.Id, Provider, googleUser.Subject, cancellationToken);
            return linked.IsFailure ? Result.Failure<UserAccount>(linked.Error) : existing;
        }

        var userId = Guid.CreateVersion7();
        var name = string.IsNullOrWhiteSpace(googleUser.Name) ? email.Split('@')[0] : googleUser.Name.Trim();
        customers.Add(Customer.Create(userId, name, email, phone: null));

        var created = await identity.CreateUserAsync(
            new NewUserAccount(userId, email, Password: null, name, PhoneNumber: null, googleUser.Picture, EmailConfirmed: true, [Roles.Customer]),
            cancellationToken);

        if (created.IsFailure)
        {
            return created;
        }

        var link = await identity.AddExternalLoginAsync(userId, Provider, googleUser.Subject, cancellationToken);
        return link.IsFailure ? Result.Failure<UserAccount>(link.Error) : created.Value;
    }

    private static Result<UserAccount> CheckAllowed(UserAccount account)
    {
        if (account.IsLocked)
        {
            return Result.Failure<UserAccount>(AuthErrors.LockedOut);
        }

        if (account.Roles.Contains(Roles.Staff) || account.Roles.Contains(Roles.Admin))
        {
            return Result.Failure<UserAccount>(StaffMustUsePassword);
        }

        return account;
    }
}
