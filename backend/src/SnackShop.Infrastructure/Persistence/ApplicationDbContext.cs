using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;
using SnackShop.Domain.Settings;
using SnackShop.Infrastructure.Identity;

namespace SnackShop.Infrastructure.Persistence;

/// <summary>
/// Single DbContext for the whole application, Identity included, so one migration history and one transaction scope.
/// Table/column names are snake_case via EFCore.NamingConventions (configured in DependencyInjection).
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public const string OrderNumberSequence = "seq_order_number";

    public DbSet<ShopSettings> ShopSettingsSet => Set<ShopSettings>();

    public DbSet<Category> CategorySet => Set<Category>();

    public DbSet<Product> ProductSet => Set<Product>();

    public DbSet<StockItem> StockItemSet => Set<StockItem>();

    public DbSet<StockMovement> StockMovementSet => Set<StockMovement>();

    public DbSet<Customer> CustomerSet => Set<Customer>();

    public DbSet<Cart> CartSet => Set<Cart>();

    public DbSet<Order> OrderSet => Set<Order>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    IQueryable<ShopSettings> IApplicationDbContext.ShopSettings => ShopSettingsSet;

    IQueryable<Category> IApplicationDbContext.Categories => CategorySet;

    IQueryable<Product> IApplicationDbContext.Products => ProductSet;

    IQueryable<StockItem> IApplicationDbContext.StockItems => StockItemSet;

    IQueryable<StockMovement> IApplicationDbContext.StockMovements => StockMovementSet;

    IQueryable<Customer> IApplicationDbContext.Customers => CustomerSet;

    IQueryable<Cart> IApplicationDbContext.Carts => CartSet;

    IQueryable<Order> IApplicationDbContext.Orders => OrderSet;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity sets explicit PascalCase table names that the naming convention does not rewrite.
        builder.Entity<ApplicationUser>().ToTable("asp_net_users");
        builder.Entity<ApplicationRole>().ToTable("asp_net_roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("asp_net_user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("asp_net_user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("asp_net_user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("asp_net_user_tokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("asp_net_role_claims");
        builder.Entity<ApplicationUser>().Property(u => u.FullName).HasMaxLength(100);
        builder.Entity<ApplicationUser>().Property(u => u.AvatarUrl).HasMaxLength(500);

        builder.HasSequence<long>(OrderNumberSequence).StartsAt(1).IncrementsBy(1);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes().Where(t => !t.IsOwned()))
        {
            // Domain events are in-memory only.
            if (typeof(AggregateRoot).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType).Ignore(nameof(AggregateRoot.DomainEvents));
            }

            // Ids are created by the domain (Guid v7). Without this, EF treats a new child added to a tracked
            // aggregate as an existing row (because its key is already set) and issues an UPDATE.
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType).Property(nameof(Entity.Id)).ValueGeneratedNever();
            }
        }
    }
}
