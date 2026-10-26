using SnackShop.Application.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Abstractions.Services;

/// <summary>Account data exposed to the application layer. Identity types never leave Infrastructure.</summary>
public sealed record UserAccount(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    string? AvatarUrl,
    bool EmailConfirmed,
    bool IsLocked,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAt);

/// <summary>Data for a new account. <see cref="Password"/> is null for external (Google) sign-ups.</summary>
public sealed record NewUserAccount(
    Guid Id,
    string Email,
    string? Password,
    string FullName,
    string? PhoneNumber,
    string? AvatarUrl,
    bool EmailConfirmed,
    IReadOnlyCollection<string> Roles);

/// <summary>
/// Account management backed by ASP.NET Core Identity. Methods that change accounts persist immediately
/// (Identity saves the shared DbContext), so call them after staging any related domain changes.
/// </summary>
public interface IIdentityService
{
    Task<Result<UserAccount>> CreateUserAsync(NewUserAccount account, CancellationToken cancellationToken);

    /// <summary>Verifies email + password, applying lockout. Errors are deliberately generic.</summary>
    Task<Result<UserAccount>> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken);

    Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserAccount?> FindByExternalLoginAsync(string provider, string providerKey, CancellationToken cancellationToken);

    Task<Result> AddExternalLoginAsync(Guid userId, string provider, string providerKey, CancellationToken cancellationToken);

    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);

    /// <summary>Returns null when no account has the email (callers must not reveal that).</summary>
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    Task<Result> UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber, CancellationToken cancellationToken);

    Task<Result> SetRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);

    Task<Result> SetLockedAsync(Guid userId, bool locked, CancellationToken cancellationToken);

    Task<PagedResponse<UserAccount>> ListUsersAsync(string? role, string? search, int page, int pageSize, CancellationToken cancellationToken);
}
