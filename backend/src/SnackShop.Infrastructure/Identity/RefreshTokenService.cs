using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Infrastructure.Persistence;

namespace SnackShop.Infrastructure.Identity;

/// <summary>
/// Refresh token rotation with reuse detection. Only SHA-256 hashes are stored.
/// A token rotated less than <see cref="ReuseGracePeriod"/> ago is treated as a benign race (two tabs refreshing
/// at once) rather than theft, so it fails without closing every session.
/// </summary>
public sealed partial class RefreshTokenService(
    ApplicationDbContext db,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
    public const string ReasonRotated = "Rotated";
    public const string ReasonLogout = "Logout";
    public const string ReasonReuse = "Reuse";
    public static readonly TimeSpan ReuseGracePeriod = TimeSpan.FromSeconds(30);

    public Task<IssuedRefreshToken> IssueAsync(Guid userId, CancellationToken cancellationToken)
    {
        var (token, entity) = Create(userId);
        db.RefreshTokens.Add(entity);
        return Task.FromResult(new IssuedRefreshToken(token, entity.ExpiresAt));
    }

    public async Task<Result<RotatedRefreshToken>> RotateAsync(string token, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var hash = Hash(token);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existing is null)
        {
            return Result.Failure<RotatedRefreshToken>(AuthErrors.InvalidRefreshToken);
        }

        if (existing.RevokedAt is { } revokedAt)
        {
            if (existing.RevokedReason == ReasonRotated && now - revokedAt > ReuseGracePeriod)
            {
                await RevokeAllCoreAsync(existing.UserId, ReasonReuse, now, cancellationToken);
                await db.SaveChangesAsync(cancellationToken); // must persist even though the request fails
                LogReuse(logger, existing.UserId);
                return Result.Failure<RotatedRefreshToken>(AuthErrors.RefreshTokenReused);
            }

            return Result.Failure<RotatedRefreshToken>(AuthErrors.InvalidRefreshToken);
        }

        if (existing.ExpiresAt <= now)
        {
            return Result.Failure<RotatedRefreshToken>(AuthErrors.InvalidRefreshToken);
        }

        var (newToken, newEntity) = Create(existing.UserId);

        // Atomic compare-and-set: of two parallel refreshes with the same token, exactly one wins.
        // (Executed immediately; if the request fails afterwards the user just has to sign in again.)
        var revoked = await db.RefreshTokens
            .Where(t => t.Id == existing.Id && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, now)
                .SetProperty(t => t.RevokedReason, ReasonRotated)
                .SetProperty(t => t.ReplacedByTokenHash, newEntity.TokenHash), cancellationToken);

        if (revoked == 0)
        {
            return Result.Failure<RotatedRefreshToken>(AuthErrors.InvalidRefreshToken);
        }

        db.RefreshTokens.Add(newEntity);

        return new RotatedRefreshToken(existing.UserId, new IssuedRefreshToken(newToken, newEntity.ExpiresAt));
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        var hash = Hash(token);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash && t.RevokedAt == null, cancellationToken);
        if (existing is not null)
        {
            existing.RevokedAt = timeProvider.GetUtcNow();
            existing.RevokedReason = ReasonLogout;
        }
    }

    public Task RevokeAllForUserAsync(Guid userId, string reason, CancellationToken cancellationToken) =>
        RevokeAllCoreAsync(userId, reason, timeProvider.GetUtcNow(), cancellationToken);

    /// <summary>Deletes tokens that expired more than 30 days ago (called by the maintenance job).</summary>
    public Task<int> PurgeExpiredAsync(CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow().AddDays(-30);
        return db.RefreshTokens.Where(t => t.ExpiresAt < cutoff).ExecuteDeleteAsync(cancellationToken);
    }

    internal static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private async Task RevokeAllCoreAsync(Guid userId, string reason, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var active = await db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var t in active)
        {
            t.RevokedAt = now;
            t.RevokedReason = reason;
        }
    }

    private (string Token, RefreshToken Entity) Create(Guid userId)
    {
        var now = timeProvider.GetUtcNow();
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        var http = httpContextAccessor.HttpContext;
        var userAgent = http?.Request.Headers.UserAgent.ToString();

        return (token, new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenHash = Hash(token),
            CreatedAt = now,
            ExpiresAt = now.AddDays(jwtOptions.Value.RefreshTokenDays),
            CreatedByIp = http?.Connection.RemoteIpAddress?.ToString(),
            DeviceLabel = string.IsNullOrEmpty(userAgent) ? null : userAgent[..Math.Min(userAgent.Length, 200)],
        });
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Refresh token reuse detected for user {UserId}; all sessions revoked")]
    private static partial void LogReuse(ILogger logger, Guid userId);
}
