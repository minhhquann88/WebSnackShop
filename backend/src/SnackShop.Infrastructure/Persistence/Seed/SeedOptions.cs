using System.ComponentModel.DataAnnotations;

namespace SnackShop.Infrastructure.Persistence.Seed;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    [Required]
    [EmailAddress]
    public string AdminEmail { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string AdminPassword { get; init; } = string.Empty;

    public string AdminFullName { get; init; } = "Administrator";
}
