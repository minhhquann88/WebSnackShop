using SnackShop.Domain.Common;

namespace SnackShop.Api.Extensions;

/// <summary>Maps a domain <see cref="Result"/> to HTTP (docs/05 §2 and §4).</summary>
public static class ResultExtensions
{
    private const string ErrorTypeBaseUri = "https://snackshop.vn/errors/";

    /// <summary>204 on success, problem details on failure.</summary>
    public static IResult ToNoContent(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.NoContent() : ToProblem(result.Error);
    }

    /// <summary>200 with the value on success, problem details on failure.</summary>
    public static IResult ToOk<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : ToProblem(result.Error);
    }

    /// <summary>201 with a Location header on success, problem details on failure.</summary>
    public static IResult ToCreated<T>(this Result<T> result, Func<T, string> location)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);
        return result.IsSuccess ? TypedResults.Created(location(result.Value), result.Value) : ToProblem(result.Error);
    }

    public static IResult ToProblem(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error is ValidationError validation)
        {
            return TypedResults.ValidationProblem(
                errors: validation.Errors.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal),
                title: validation.Code,
                detail: validation.Message,
                type: ErrorTypeBaseUri + "validation");
        }

        return TypedResults.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: ToStatusCode(error.Type),
            type: ErrorTypeBaseUri + ToSlug(error.Code));
    }

    public static int ToStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError,
    };

    /// <summary>"Orders.InvalidStatusTransition" → "orders/invalid-status-transition".</summary>
    internal static string ToSlug(string code)
    {
        var parts = code.Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => string.Concat(p.Select((c, i) => char.IsUpper(c) && i > 0 ? "-" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString())));
        return string.Join('/', parts);
    }
}
