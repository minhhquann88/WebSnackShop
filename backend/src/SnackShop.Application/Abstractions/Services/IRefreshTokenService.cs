using SnackShop.Domain.Common;

namespace SnackShop.Application.Abstractions.Services;

public sealed record IssuedRefreshToken(string Token, DateTimeOffset ExpiresAt);

public sealed record RotatedRefreshToken(Guid UserId, IssuedRefreshToken NewToken);

/// <summary>
/// Opaque refresh tokens stored as SHA-256 hashes, rotated on every use, with reuse detection
/// (docs/07-auth-security.md §5). New tokens are staged on the DbContext and committed by the unit of work.
/// </summary>
public interface IRefreshTokenService
{
    Task<IssuedRefreshToken> IssueAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Exchanges a valid token for a new one. Presenting an already-revoked token revokes every token of
    /// that user (persisted immediately) and fails with Auth.RefreshTokenReused.
    /// </summary>
    Task<Result<RotatedRefreshToken>> RotateAsync(string token, CancellationToken cancellationToken);

    Task RevokeAsync(string token, CancellationToken cancellationToken);

    Task RevokeAllForUserAsync(Guid userId, string reason, CancellationToken cancellationToken);
}
