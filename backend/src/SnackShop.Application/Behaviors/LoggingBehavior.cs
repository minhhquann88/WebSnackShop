using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Behaviors;

/// <summary>Logs every request name, duration and outcome. Runs first in the pipeline.</summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var start = Stopwatch.GetTimestamp();

        LogStarted(logger, requestName);

        TResponse response;
        try
        {
            response = await next(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogFailed(logger, ex, requestName, Stopwatch.GetElapsedTime(start).TotalMilliseconds);
            throw;
        }

        var elapsedMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        if (response.IsSuccess)
        {
            LogCompleted(logger, requestName, elapsedMs);
        }
        else
        {
            LogCompletedWithError(logger, requestName, response.Error.Code, elapsedMs);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Handling {RequestName}")]
    private static partial void LogStarted(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs:0.0} ms")]
    private static partial void LogCompleted(ILogger logger, string requestName, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} with error {ErrorCode} in {ElapsedMs:0.0} ms")]
    private static partial void LogCompletedWithError(ILogger logger, string requestName, string errorCode, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception in {RequestName} after {ElapsedMs:0.0} ms")]
    private static partial void LogFailed(ILogger logger, Exception exception, string requestName, double elapsedMs);
}
