using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Customers;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;
using SnackShop.Domain.Settings;
using SnackShop.Infrastructure.BackgroundJobs;
using SnackShop.Infrastructure.Email;
using SnackShop.Infrastructure.Identity;
using SnackShop.Infrastructure.Persistence;
using SnackShop.Infrastructure.Persistence.Interceptors;
using SnackShop.Infrastructure.Persistence.Repositories;
using SnackShop.Infrastructure.Persistence.Seed;
using SnackShop.Infrastructure.Services;
using SnackShop.Infrastructure.Storage;

namespace SnackShop.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";
    public const string DataProtectionKeysPathKey = "DataProtection:KeysPath";

    /// <summary>Single entry point: everything the API needs from the infrastructure layer.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        AddPersistence(services, configuration, environment);
        AddIdentity(services, configuration);
        AddIntegrations(services);

        services.AddOptions<SeedOptions>()
            .BindConfiguration(SeedOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddScoped<DbSeeder>();
        services.AddScoped<DevDataSeeder>();

        services.AddOptions<JobOptions>().BindConfiguration(JobOptions.SectionName);
        services.AddHostedService<MaintenanceJob>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is missing.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"));
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IShopSettingsRepository, ShopSettingsRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockItemRepository, StockItemRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
    }

    private static void AddIdentity(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(1));

        // Password-reset and email-confirmation tokens are protected with these keys. In containers the default
        // location is ephemeral, so every restart would invalidate links already emailed: persist them to a volume.
        var dataProtection = services.AddDataProtection().SetApplicationName("SnackShop");
        if (configuration[DataProtectionKeysPathKey] is { Length: > 0 } keysPath)
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<IRefreshTokenService>(sp => sp.GetRequiredService<RefreshTokenService>());

        services.AddOptions<GoogleOptions>().BindConfiguration(GoogleOptions.SectionName);
        services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();
    }

    private static void AddIntegrations(IServiceCollection services)
    {
        services.AddOptions<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<EmailQueue>();
        services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<EmailQueue>());
        services.AddHostedService<EmailDispatcher>();

        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<LocalFileStorage>();
        services.AddSingleton<IFileStorage>(sp => sp.GetRequiredService<LocalFileStorage>());

        services.AddOptions<FrontendOptions>()
            .BindConfiguration(FrontendOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IAppLinks, AppLinks>();
    }
}
