using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Infrastructure.Persistence;

namespace SnackShop.Infrastructure.Identity;

/// <summary>ASP.NET Core Identity adapter. UserManager persists changes to the shared DbContext immediately.</summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    TimeProvider timeProvider) : IIdentityService
{
    public async Task<Result<UserAccount>> CreateUserAsync(NewUserAccount account, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account);

        var user = new ApplicationUser
        {
            Id = account.Id,
            UserName = account.Email,
            Email = account.Email,
            EmailConfirmed = account.EmailConfirmed,
            FullName = account.FullName,
            PhoneNumber = account.PhoneNumber,
            AvatarUrl = account.AvatarUrl,
            CreatedAt = timeProvider.GetUtcNow(),
            LockoutEnabled = true,
        };

        var created = account.Password is null
            ? await userManager.CreateAsync(user)
            : await userManager.CreateAsync(user, account.Password);

        if (!created.Succeeded)
        {
            return Result.Failure<UserAccount>(MapErrors(created));
        }

        var roles = await userManager.AddToRolesAsync(user, account.Roles);
        if (!roles.Succeeded)
        {
            return Result.Failure<UserAccount>(MapErrors(roles));
        }

        return await ToAccountAsync(user);
    }

    public async Task<Result<UserAccount>> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.HasPasswordAsync(user))
        {
            return Result.Failure<UserAccount>(AuthErrors.InvalidCredentials);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<UserAccount>(AuthErrors.LockedOut);
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return Result.Failure<UserAccount>(
                await userManager.IsLockedOutAsync(user) ? AuthErrors.LockedOut : AuthErrors.InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await ToAccountAsync(user);
    }

    public async Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : await ToAccountAsync(user);
    }

    public async Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is null ? null : await ToAccountAsync(user);
    }

    public async Task<UserAccount?> FindByExternalLoginAsync(string provider, string providerKey, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByLoginAsync(provider, providerKey);
        return user is null ? null : await ToAccountAsync(user);
    }

    public async Task<Result> AddExternalLoginAsync(Guid userId, string provider, string providerKey, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var added = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
        if (added.Succeeded && !user.EmailConfirmed)
        {
            // The provider verified the email address.
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        return added.Succeeded ? Result.Success() : Result.Failure(MapErrors(added));
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"User {userId} does not exist.");
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(AuthErrors.InvalidToken);
        }

        var confirmed = await userManager.ConfirmEmailAsync(user, token);
        return confirmed.Succeeded ? Result.Success() : Result.Failure(AuthErrors.InvalidToken);
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is null ? null : await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure(AuthErrors.InvalidToken);
        }

        var reset = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (reset.Succeeded)
        {
            await userManager.ResetAccessFailedCountAsync(user);

            // Clear only the temporary failed-login lockout; an admin lock must survive a password reset.
            if (!IsAdminLocked(user))
            {
                await userManager.SetLockoutEndDateAsync(user, null);
            }

            return Result.Success();
        }

        return reset.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken))
            ? Result.Failure(AuthErrors.InvalidToken)
            : Result.Failure(MapErrors(reset));
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var changed = await userManager.HasPasswordAsync(user)
            ? await userManager.ChangePasswordAsync(user, currentPassword, newPassword)
            : await userManager.AddPasswordAsync(user, newPassword);

        if (changed.Succeeded)
        {
            return Result.Success();
        }

        return changed.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordMismatch))
            ? Result.Failure(AuthErrors.WrongPassword)
            : Result.Failure(MapErrors(changed));
    }

    public async Task<Result> UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        user.FullName = fullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        var updated = await userManager.UpdateAsync(user);
        return updated.Succeeded ? Result.Success() : Result.Failure(MapErrors(updated));
    }

    public async Task<Result> SetRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        // Remove and add are two saves; keep them atomic so a failure never leaves the user without roles.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var current = await userManager.GetRolesAsync(user);
        var removed = await userManager.RemoveFromRolesAsync(user, current.Except(roles));
        var added = removed.Succeeded ? await userManager.AddToRolesAsync(user, roles.Except(current)) : removed;

        if (!removed.Succeeded || !added.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure(MapErrors(removed.Succeeded ? added : removed));
        }

        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SetLockedAsync(Guid userId, bool locked, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        await userManager.SetLockoutEnabledAsync(user, true);
        var result = await userManager.SetLockoutEndDateAsync(user, locked ? AdminLockUntil : null);
        return result.Succeeded ? Result.Success() : Result.Failure(MapErrors(result));
    }

    public async Task<PagedResponse<UserAccount>> ListUsersAsync(string? role, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var users = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalizedRole = role.ToUpperInvariant();
            users = users.Where(u => db.UserRoles.Any(ur => ur.UserId == u.Id
                && db.Roles.Any(r => r.Id == ur.RoleId && r.NormalizedName == normalizedRole)));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
#pragma warning disable CA1304, CA1311, CA1862 // Translated to SQL upper(); .NET culture rules do not apply.
            users = users.Where(u => u.NormalizedEmail!.Contains(term) || u.FullName.ToUpper().Contains(term));
#pragma warning restore CA1304, CA1311, CA1862
        }

        return await users
            .OrderBy(u => u.Email)
            .Select(u => new UserAccount(
                u.Id,
                u.Email!,
                u.FullName,
                u.PhoneNumber,
                u.AvatarUrl,
                u.EmailConfirmed,
                u.LockoutEnd != null && u.LockoutEnd > now,
                db.UserRoles.Where(ur => ur.UserId == u.Id)
                    .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
                    .OrderBy(n => n)
                    .ToList(),
                u.CreatedAt))
            .ToPagedResponseAsync(page, pageSize, cancellationToken);
    }

    /// <summary>Lockout end used for accounts disabled by an admin (failed-login lockouts last minutes).</summary>
    public static readonly DateTimeOffset AdminLockUntil = DateTimeOffset.MaxValue;

    private static bool IsAdminLocked(ApplicationUser user) =>
        user.LockoutEnd is { } end && end.Year >= AdminLockUntil.Year;

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var isLocked = user.LockoutEnd is { } end && end > timeProvider.GetUtcNow();
        return new UserAccount(user.Id, user.Email!, user.FullName, user.PhoneNumber, user.AvatarUrl,
            user.EmailConfirmed, isLocked, roles.Order(StringComparer.Ordinal).ToList(), user.CreatedAt);
    }

    private static Error MapErrors(IdentityResult result)
    {
        if (result.Errors.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateEmail) or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            return AuthErrors.EmailTaken;
        }

        var passwordErrors = result.Errors.Where(e => e.Code.StartsWith("Password", StringComparison.Ordinal)).Select(e => e.Description).ToList();
        if (passwordErrors.Count > 0)
        {
            return AuthErrors.PasswordRejected(passwordErrors);
        }

        return Error.Validation("Auth.IdentityError", string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
