using SnackShop.Application.Abstractions.Services;

namespace SnackShop.Application.Features.Auth;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string? PhoneNumber,
    string? AvatarUrl,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles)
{
    public static UserResponse From(UserAccount account) => new(
        account.Id, account.Email, account.FullName, account.PhoneNumber, account.AvatarUrl, account.EmailConfirmed, account.Roles);
}

/// <summary>Result of every sign-in flow: a short-lived access token and a rotating refresh token.</summary>
public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    UserResponse User);
