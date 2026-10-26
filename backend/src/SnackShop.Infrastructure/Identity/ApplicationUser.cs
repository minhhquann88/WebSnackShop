using Microsoft.AspNetCore.Identity;

namespace SnackShop.Infrastructure.Identity;

/// <summary>Identity account. Domain code never sees this type; the Customer aggregate shares its Id.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName)
        : base(roleName)
    {
    }
}
