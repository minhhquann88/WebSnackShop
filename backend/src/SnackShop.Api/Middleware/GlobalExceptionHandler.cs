using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SnackShop.Api.Middleware;

/// <summary>
/// Last line of defence. Malformed requests (bad JSON, wrong types, oversized bodies) become 400/413 problems;
/// anything else becomes a 500 with a traceId. Exception details never reach the client; they are in the logs.
/// </summary>
internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception is BadHttpRequestException badRequest
            ? BadRequestProblem(badRequest)
            : ServerErrorProblem(httpContext, exception);

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }

    private ProblemDetails BadRequestProblem(BadHttpRequestException exception)
    {
        LogBadRequest(logger, exception.Message);
        var tooLarge = exception.StatusCode == StatusCodes.Status413PayloadTooLarge;

        return new ProblemDetails
        {
            Status = tooLarge ? StatusCodes.Status413PayloadTooLarge : StatusCodes.Status400BadRequest,
            Title = tooLarge ? "Request.TooLarge" : "Request.Malformed",
            Detail = tooLarge ? "The request body is too large." : "The request body or parameters could not be read. Check the JSON format and value types.",
            Type = "https://snackshop.vn/errors/request/" + (tooLarge ? "too-large" : "malformed"),
        };
    }

    private ProblemDetails ServerErrorProblem(HttpContext httpContext, Exception exception)
    {
        LogUnhandled(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server.UnexpectedError",
            Detail = "An unexpected error occurred.",
            Type = "https://snackshop.vn/errors/server/unexpected-error",
        };
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string method, PathString path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Rejected malformed request: {Reason}")]
    private static partial void LogBadRequest(ILogger logger, string reason);
}
