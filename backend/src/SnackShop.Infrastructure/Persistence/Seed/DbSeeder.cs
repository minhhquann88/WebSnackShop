using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnackShop.Application.Common.Authorization;
using SnackShop.Domain.Settings;
using SnackShop.Infrastructure.Identity;

namespace SnackShop.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent baseline data required in every environment: roles, first admin, settings row.
/// Development-only sample data (products, orders) will live in a separate DevDataSeeder from Phase 1.
/// </summary>
public sealed partial class DbSeeder(
    ApplicationDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptions<SeedOptions> seedOptions,
    TimeProvider timeProvider,
    ILogger<DbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
        await SeedShopSettingsAsync(cancellationToken);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
                LogRoleCreated(logger, role);
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var options = seedOptions.Value;
        var existing = await userManager.FindByEmailAsync(options.AdminEmail);
        if (existing is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = options.AdminEmail,
            Email = options.AdminEmail,
            EmailConfirmed = true,
            FullName = options.AdminFullName,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        var result = await userManager.CreateAsync(admin, options.AdminPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to create seed admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRolesAsync(admin, [Roles.Admin, Roles.Staff]);
        LogAdminCreated(logger, options.AdminEmail);
    }

    private async Task SeedShopSettingsAsync(CancellationToken cancellationToken)
    {
        var exists = await dbContext.ShopSettingsSet.AnyAsync(s => s.Id == ShopSettings.SingletonId, cancellationToken);
        if (exists)
        {
            return;
        }

        var settings = ShopSettings.CreateDefault();
        dbContext.ShopSettingsSet.Add(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
        LogSettingsCreated(logger);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded role {Role}")]
    private static partial void LogRoleCreated(ILogger logger, string role);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded admin user {Email}")]
    private static partial void LogAdminCreated(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded default shop settings")]
    private static partial void LogSettingsCreated(ILogger logger);
}
