using System.ComponentModel.DataAnnotations;

namespace SnackShop.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    public string Host { get; init; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; init; } = 1025;

    public string? Username { get; init; }

    public string? Password { get; init; }

    /// <summary>Use STARTTLS when the server offers it (port 587). Mailpit in development does not.</summary>
    public bool UseStartTls { get; init; }

    [Required]
    public string From { get; init; } = "SnackShop <no-reply@snackshop.local>";

    /// <summary>When false, emails are only logged (useful for CI and tests).</summary>
    public bool Enabled { get; init; } = true;
}
