using Serilog;
using Serilog.Formatting.Compact;
using SnackShop.Api.Extensions;
using SnackShop.Application;
using SnackShop.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "SnackShop.Api"));

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration, builder.Environment)
        .AddApiServices(builder.Configuration, builder.Environment);

    // `dotnet run -- --export-openapi <path>` writes the OpenAPI document and exits (used by CI and the frontend client generator).
    var exportIndex = Array.IndexOf(args, "--export-openapi");
    var isExport = exportIndex >= 0;
    if (isExport)
    {
        // Bind to an ephemeral loopback port so export never clashes with a running dev server; no background jobs.
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration["Jobs:Enabled"] = "false";
        builder.Configuration["Email:Enabled"] = "false";
    }

    var app = builder.Build();

    app.UseApiPipeline();
    app.MapEndpointGroups();

    if (isExport)
    {
        var outputPath = exportIndex + 1 < args.Length ? args[exportIndex + 1] : "openapi.json";
        await app.ExportOpenApiAsync(outputPath);
        return;
    }

    if (app.Environment.IsDevelopment())
    {
        await app.MigrateAndSeedAsync();
    }
    else if (app.Configuration.GetValue("Seed:OnStartup", true))
    {
        // Production: schema is migrated by the deploy script before the container starts; only baseline data here.
        await app.SeedBaselineAsync();
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program;
