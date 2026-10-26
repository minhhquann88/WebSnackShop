using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SnackShop.Api.Middleware;
using SnackShop.Api.OpenApi;
using SnackShop.Application.Common.Authorization;
using SnackShop.Infrastructure;
using SnackShop.Infrastructure.Identity;

namespace SnackShop.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CorsPolicyName = "Frontend";
    public const string ProxyKeyHeader = "X-Proxy-Key";
    public const string ClientIpHeader = "X-Client-IP";

    private static readonly Dictionary<int, string> FrameworkErrorCodes = new()
    {
        [StatusCodes.Status401Unauthorized] = "Auth.NotAuthenticated",
        [StatusCodes.Status403Forbidden] = "Auth.Forbidden",
        [StatusCodes.Status404NotFound] = "Http.NotFound",
        [StatusCodes.Status405MethodNotAllowed] = "Http.MethodNotAllowed",
        [StatusCodes.Status415UnsupportedMediaType] = "Http.UnsupportedMediaType",
        [StatusCodes.Status429TooManyRequests] = "Http.TooManyRequests",
    };

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            // Numbers must be JSON numbers ("45000" as a string is rejected). Also keeps OpenAPI schemas as plain number/integer.
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });

        services.AddProblemDetails(options => options.CustomizeProblemDetails = ctx =>
        {
            // Responses produced by the framework (401/403 from auth, 404 for unknown routes, 429 from the limiter)
            // only carry the HTTP reason phrase. Give them stable codes like every domain error (docs/05 §4).
            var status = ctx.ProblemDetails.Status ?? ctx.HttpContext.Response.StatusCode;
            var isFrameworkDefault = ctx.Exception is null
                && string.Equals(ctx.ProblemDetails.Title, ReasonPhrases.GetReasonPhrase(status), StringComparison.Ordinal);
            if (isFrameworkDefault && FrameworkErrorCodes.TryGetValue(status, out var code))
            {
                ctx.ProblemDetails.Title = code;
            }
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Outside Development, minimal APIs answer malformed bodies with an empty 400. Throwing instead lets
        // GlobalExceptionHandler return the same problem+json (Request.Malformed) in every environment.
        services.Configure<Microsoft.AspNetCore.Routing.RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<AuthorizedOperationTransformer>();
            options.AddOperationTransformer<CamelCaseQueryParametersTransformer>();
            options.AddSchemaTransformer<PagedResponseSchemaTransformer>();
        });

        services.AddHealthChecks()
            .AddNpgSql(
                sp => configuration.GetConnectionString(DependencyInjection.ConnectionStringName)!,
                name: "postgres",
                tags: ["ready"]);

        AddCors(services, configuration, environment);
        AddAuthenticationAndAuthorization(services);
        AddRateLimiting(services, configuration);

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            // Behind Nginx in the same Docker network; trust the immediate proxy.
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
        {
            if (origins.Length == 0 && environment.IsDevelopment())
            {
                policy.SetIsOriginAllowed(_ => true);
            }
            else
            {
                policy.WithOrigins(origins);
            }

            policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithExposedHeaders("Location");
        }));
    }

    private static void AddAuthenticationAndAuthorization(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    NameClaimType = JwtRegisteredClaimNames.Name,
                    RoleClaimType = ClaimTypes.Role,
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Customer, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(Policies.Staff, policy => policy.RequireRole(Roles.Staff, Roles.Admin))
            .AddPolicy(Policies.Admin, policy => policy.RequireRole(Roles.Admin));
    }

    private static void AddRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        // Per-IP permits per minute (docs/05 §8). Overridable for load tests and integration tests.
        var authLimit = configuration.GetValue("RateLimiting:AuthPerMinute", 10);
        var orderLimit = configuration.GetValue("RateLimiting:PlaceOrderPerMinute", 5);
        var sessionLimit = configuration.GetValue("RateLimiting:SessionPerMinute", 120);
        var trustedProxyKey = configuration["RateLimiting:TrustedProxyKey"];

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

                return ValueTask.CompletedTask;
            };

            options.AddPolicy(RateLimitPolicies.Auth, context => RateLimitPartition.GetFixedWindowLimiter(
                ClientKey(context, trustedProxyKey),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = authLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

            options.AddPolicy(RateLimitPolicies.Session, context => RateLimitPartition.GetFixedWindowLimiter(
                ClientKey(context, trustedProxyKey),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = sessionLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

            options.AddPolicy(RateLimitPolicies.PlaceOrder, context => RateLimitPartition.GetFixedWindowLimiter(
                ClientKey(context, trustedProxyKey),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = orderLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
    }

    /// <summary>
    /// Rate-limit partition: the caller's IP, or, for the trusted Next.js BFF (which signs in on behalf of browsers),
    /// the end user's IP it forwards. Without this every sign-in through the BFF would share one bucket.
    /// </summary>
    private static string ClientKey(HttpContext context, string? trustedProxyKey)
    {
        if (!string.IsNullOrEmpty(trustedProxyKey)
            && context.Request.Headers.TryGetValue(ProxyKeyHeader, out var presented)
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(presented.ToString()), Encoding.UTF8.GetBytes(trustedProxyKey))
            && IPAddress.TryParse(context.Request.Headers[ClientIpHeader].ToString(), out var clientIp))
        {
            return clientIp.ToString();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
