namespace SnackShop.Api.Extensions;

/// <summary>OpenAPI metadata shortcuts so every endpoint documents the same error shapes.</summary>
public static class RouteHandlerBuilderExtensions
{
    /// <summary>Documents 400 (validation) plus the given problem status codes.</summary>
    public static RouteHandlerBuilder ProducesErrors(this RouteHandlerBuilder builder, params int[] statusCodes)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ProducesValidationProblem();
        foreach (var code in statusCodes)
        {
            builder.ProducesProblem(code);
        }

        return builder;
    }

    /// <summary>Documents 401/403 for protected endpoints.</summary>
    public static RouteHandlerBuilder ProducesAuthErrors(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
