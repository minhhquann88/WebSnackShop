using MediatR;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Abstractions.Messaging;

/// <summary>
/// Wraps a domain event so it can travel through MediatR without the Domain project referencing MediatR.
/// Handle with <see cref="IDomainEventHandler{TEvent}"/>.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;

/// <summary>Handles a domain event after the originating transaction has committed.</summary>
public interface IDomainEventHandler<TEvent> : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : IDomainEvent;
