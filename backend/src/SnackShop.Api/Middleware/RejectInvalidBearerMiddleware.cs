using Microsoft.AspNetCore.Mvc;

namespace SnackShop.Api.Middleware;

/// <summary>
/// A request that carries a bearer token which fails validation (expired, tampered) is rejected with 401, even on
/// anonymous endpoints. Otherwise e.g. a checkout with an expired token would silently become a guest order that is
/// not linked to the customer. Clients react by refreshing and retrying.
/// </summary>
internal sealed class RejectInvalidBearerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IProblemDetailsService problemDetails)
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        var sentBearer = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

        if (sentBearer && context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer error=\"invalid_token\"";
            await problemDetails.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Auth.InvalidToken",
                    Detail = "The access token is invalid or has expired. Refresh the session and retry.",
                    Type = "https://snackshop.vn/errors/auth/invalid-token",
                },
            });
            return;
        }

        await next(context);
    }
}
