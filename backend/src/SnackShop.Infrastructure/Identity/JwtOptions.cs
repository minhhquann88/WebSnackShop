using System.ComponentModel.DataAnnotations;

namespace SnackShop.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HS256 signing key. At least 64 bytes of randomness; only ever supplied via environment / user-secrets.</summary>
    [Required]
    [MinLength(64, ErrorMessage = "Jwt:Secret must be at least 64 characters.")]
    public string Secret { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = "snackshop-api";

    [Required]
    public string Audience { get; init; } = "snackshop-web";

    [Range(1, 60)]
    public int AccessTokenMinutes { get; init; } = 15;

    [Range(1, 90)]
    public int RefreshTokenDays { get; init; } = 14;
}
