namespace SnackShop.Application.Abstractions.Services;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>Issues signed access tokens. Refresh tokens are handled by the identity service (Phase 1).</summary>
public interface IJwtProvider
{
    AccessToken GenerateAccessToken(Guid userId, string email, string displayName, IReadOnlyCollection<string> roles);
}
