using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Common;

namespace SnackShop.Infrastructure.Persistence.Interceptors;

/// <summary>
/// After a successful SaveChanges, publishes every pending domain event through MediatR, wrapped in
/// <see cref="DomainEventNotification{TEvent}"/>. The transaction is already committed, so a failing handler is
/// logged and never turns a successful request into an error (no outbox at MVP, see docs/02 §7).
/// </summary>
public sealed partial class DispatchDomainEventsInterceptor(IPublisher publisher, ILogger<DispatchDomainEventsInterceptor> logger)
    : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        DispatchAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    private async Task DispatchAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
        {
            return;
        }

        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());

        foreach (var domainEvent in events)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;

            try
            {
                await publisher.Publish(notification, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogHandlerFailed(logger, ex, domainEvent.GetType().Name, domainEvent.EventId);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Handler for domain event {EventType} ({EventId}) failed after commit")]
    private static partial void LogHandlerFailed(ILogger logger, Exception exception, string eventType, Guid eventId);
}
