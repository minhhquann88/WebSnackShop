using SnackShop.Application.Abstractions.Services;

namespace SnackShop.Application.Features.Users;

public sealed record AdminUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsLocked,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAt)
{
    public static AdminUserResponse From(UserAccount a) =>
        new(a.Id, a.Email, a.FullName, a.PhoneNumber, a.EmailConfirmed, a.IsLocked, a.Roles, a.CreatedAt);
}
