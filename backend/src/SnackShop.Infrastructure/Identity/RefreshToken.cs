namespace SnackShop.Infrastructure.Identity;

/// <summary>
/// Persisted refresh token. Only the SHA-256 hash of the token is stored. Rotation links each token to its successor
/// so reuse of an old token can be detected (docs/07 §5).
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public string? DeviceLabel { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? RevokedReason { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
}
