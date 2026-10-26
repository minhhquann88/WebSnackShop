using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Respawn.Graph;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Infrastructure.Persistence;
using SnackShop.Infrastructure.Persistence.Seed;
using Testcontainers.PostgreSql;

namespace SnackShop.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API against a throw-away PostgreSQL container. One container per test run
/// (collection fixture); tables are reset with Respawn and re-seeded before each test class.
/// Only outbound adapters are faked (email, Google). Requires Docker.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string JwtSecret = "integration-test-secret-0123456789abcdef0123456789abcdef0123456789abcdef";
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin12345";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("snackshop_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly string _uploadsPath = Path.Combine(Path.GetTempPath(), "snackshop-tests-uploads", Guid.NewGuid().ToString("N"));
    private Respawner? _respawner;

    public FakeEmailSender Emails { get; } = new();

    public string ConnectionString => _postgres.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        // Force host creation so services (and the DbContext) are available.
        _ = Server;

        await using (var scope = Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
        }

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Table("__ef_migrations_history")],
        });

        await SeedAsync();
    }

    /// <summary>Wipes every table (except migrations history), re-seeds baseline data and clears captured emails.</summary>
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner!.ResetAsync(connection);
        await SeedAsync();
        Emails.Clear();
    }

    public async Task SeedAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
    }

    /// <summary>Runs <paramref name="work"/> with a fresh DbContext (for arranging data and asserting persisted state).</summary>
    public async Task<T> WithDbAsync<T>(Func<ApplicationDbContext, Task<T>> work)
    {
        ArgumentNullException.ThrowIfNull(work);
        await using var scope = Services.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
        builder.UseSetting("Jwt:Secret", JwtSecret);
        builder.UseSetting("Seed:AdminEmail", AdminEmail);
        builder.UseSetting("Seed:AdminPassword", AdminPassword);
        builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
        builder.UseSetting("Jobs:Enabled", "false");
        builder.UseSetting("Seed:OnStartup", "false"); // the fixture migrates first, then seeds
        builder.UseSetting("Email:Enabled", "false");
        builder.UseSetting("RateLimiting:AuthPerMinute", "10000");
        builder.UseSetting("RateLimiting:PlaceOrderPerMinute", "10000");
        builder.UseSetting("RateLimiting:SessionPerMinute", "10000");
        builder.UseSetting("Storage:LocalPath", _uploadsPath);
        builder.UseSetting("Storage:PublicBaseUrl", "http://localhost/uploads");
        builder.UseSetting("Google:ClientId", "test-client-id");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
            services.RemoveAll<IGoogleTokenVerifier>();
            services.AddSingleton<IGoogleTokenVerifier, FakeGoogleTokenVerifier>();
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        if (Directory.Exists(_uploadsPath))
        {
            Directory.Delete(_uploadsPath, recursive: true);
        }

        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}
