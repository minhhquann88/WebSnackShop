using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnackShop.Application.Features.Orders.ExpireUnpaidOrders;
using SnackShop.Infrastructure.Identity;

namespace SnackShop.Infrastructure.BackgroundJobs;

public sealed class JobOptions
{
    public const string SectionName = "Jobs";

    /// <summary>Disable in tests so jobs never race with assertions.</summary>
    public bool Enabled { get; init; } = true;

    public int IntervalMinutes { get; init; } = 10;
}

/// <summary>
/// Periodic housekeeping in a fresh DI scope: cancels expired unpaid bank-transfer orders (releasing stock)
/// and purges long-expired refresh tokens. Single instance per process; safe to run on one VPS.
/// </summary>
public sealed partial class MaintenanceJob(
    IServiceScopeFactory scopeFactory,
    IOptions<JobOptions> options,
    TimeProvider timeProvider,
    ILogger<MaintenanceJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1, options.Value.IntervalMinutes)), timeProvider);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>Expires each overdue order in its own scope/transaction; failures are logged and skipped.</summary>
    private async Task ExpireUnpaidOrdersAsync(ISender sender, CancellationToken cancellationToken)
    {
        var ids = await sender.Send(new GetExpiredUnpaidOrderIdsQuery(), cancellationToken);
        if (ids.IsFailure)
        {
            return;
        }

        var expired = 0;
        foreach (var orderId in ids.Value)
        {
            await using var orderScope = scopeFactory.CreateAsyncScope();
            var result = await orderScope.ServiceProvider.GetRequiredService<ISender>()
                .Send(new ExpireUnpaidOrderCommand(orderId), cancellationToken);

            if (result.IsFailure)
            {
                LogExpireFailed(logger, orderId, result.Error.Code);
            }
            else if (result.Value)
            {
                expired++;
            }
        }

        if (expired > 0)
        {
            LogExpired(logger, expired);
        }
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            await ExpireUnpaidOrdersAsync(scope.ServiceProvider.GetRequiredService<ISender>(), cancellationToken);

            var purged = await scope.ServiceProvider.GetRequiredService<RefreshTokenService>().PurgeExpiredAsync(cancellationToken);
            if (purged > 0)
            {
                LogPurged(logger, purged);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogFailed(logger, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Cancelled {Count} unpaid bank-transfer order(s) past their deadline")]
    private static partial void LogExpired(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not expire order {OrderId}: {ErrorCode}")]
    private static partial void LogExpireFailed(ILogger logger, Guid orderId, string errorCode);

    [LoggerMessage(Level = LogLevel.Information, Message = "Purged {Count} expired refresh token(s)")]
    private static partial void LogPurged(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Maintenance job run failed")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
