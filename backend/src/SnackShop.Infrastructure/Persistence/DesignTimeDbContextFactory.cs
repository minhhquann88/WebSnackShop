using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SnackShop.Infrastructure.Persistence;

/// <summary>
/// Used only by `dotnet ef` (migrations add/script/bundle) so the tools never have to boot the API host,
/// read secrets or reach a database. Must configure the model exactly like DependencyInjection does.
/// Connection string: env var SNACKSHOP_DESIGN_CONNECTION, else a local default (only `database update` connects).
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string DefaultConnection = "Host=localhost;Port=5432;Database=snackshop_dev;Username=snackshop;Password=snackshop";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SNACKSHOP_DESIGN_CONNECTION") ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options);
    }
}
