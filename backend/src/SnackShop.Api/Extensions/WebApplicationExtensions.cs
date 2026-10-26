using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using SnackShop.Infrastructure.Persistence;
using SnackShop.Infrastructure.Persistence.Seed;
using SnackShop.Infrastructure.Storage;

namespace SnackShop.Api.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>Middleware order matters; keep it in one place.</summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();

        // Outermost so the request log records the final status written by the exception handler
        // (e.g. 400 for malformed JSON) instead of logging every handled exception as a 500.
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        UseUploads(app);

        if (!app.Environment.IsProduction())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options
                .WithTitle("SnackShop API")
                .WithTheme(ScalarTheme.Default)
                .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch));
        }

        app.UseCors(ServiceCollectionExtensions.CorsPolicyName);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseMiddleware<Middleware.RejectInvalidBearerMiddleware>();
        app.UseAuthorization();

        app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false })
            .AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") })
            .AllowAnonymous();

        return app;
    }

    /// <summary>Development only: apply pending migrations, seed baseline data and the sample catalog.</summary>
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
        await scope.ServiceProvider.GetRequiredService<DevDataSeeder>().SeedAsync();
    }

    /// <summary>Idempotent baseline data for every environment: roles, first admin, shop settings.</summary>
    public static async Task SeedBaselineAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
    }

    /// <summary>
    /// Starts the host briefly (endpoints are only visible to the OpenAPI generator once the pipeline is built),
    /// writes the OpenAPI document to disk, then stops. Does not touch the database.
    /// </summary>
    public static async Task ExportOpenApiAsync(this WebApplication app, string outputPath)
    {
        await app.StartAsync();
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            // Registered as a keyed service per document name by AddOpenApi() (default document: "v1").
            var provider = scope.ServiceProvider.GetRequiredKeyedService<IOpenApiDocumentProvider>("v1");
            var document = await provider.GetOpenApiDocumentAsync();

            var fullPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await using (var stream = File.Create(fullPath))
            {
                await document.SerializeAsJsonAsync(stream, OpenApiSpecVersion.OpenApi3_1);
            }

            Console.WriteLine($"OpenAPI document written to {fullPath}");
        }
        finally
        {
            await app.StopAsync();
        }
    }

    /// <summary>Serves uploaded images from local storage. In production Nginx serves the same folder directly.</summary>
    private static void UseUploads(WebApplication app)
    {
        var root = app.Services.GetRequiredService<LocalFileStorage>().RootPath;
        Directory.CreateDirectory(root);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = StorageOptions.RequestPath,
            ServeUnknownFileTypes = false,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";
                ctx.Context.Response.Headers.XContentTypeOptions = "nosniff";
            },
        });
    }
}
