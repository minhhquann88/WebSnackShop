using SnackShop.Application.Abstractions.Services;

namespace SnackShop.Application.Features.Auth.Common;

/// <summary>Creates the token pair for a signed-in account. The refresh token is committed by the unit of work.</summary>
public sealed class AuthSessionIssuer(IJwtProvider jwtProvider, IRefreshTokenService refreshTokens)
{
    public async Task<AuthResponse> IssueAsync(UserAccount account, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account);

        var access = jwtProvider.GenerateAccessToken(account.Id, account.Email, account.FullName, account.Roles);
        var refresh = await refreshTokens.IssueAsync(account.Id, cancellationToken);
        return new AuthResponse(access.Token, access.ExpiresAt, refresh.Token, refresh.ExpiresAt, UserResponse.From(account));
    }

    /// <summary>Builds the response for a refresh where the new refresh token was already issued by rotation.</summary>
    public AuthResponse FromRotation(UserAccount account, IssuedRefreshToken refresh)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(refresh);

        var access = jwtProvider.GenerateAccessToken(account.Id, account.Email, account.FullName, account.Roles);
        return new AuthResponse(access.Token, access.ExpiresAt, refresh.Token, refresh.ExpiresAt, UserResponse.From(account));
    }
}
