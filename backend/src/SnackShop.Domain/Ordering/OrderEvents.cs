using SnackShop.Domain.Common;

namespace SnackShop.Domain.Ordering;

public sealed record OrderPlacedDomainEvent(Guid OrderId, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

public sealed record OrderConfirmedDomainEvent(Guid OrderId, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

public sealed record OrderShippedDomainEvent(Guid OrderId, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

public sealed record OrderDeliveredDomainEvent(Guid OrderId, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);

public sealed record OrderCancelledDomainEvent(Guid OrderId, string Reason, DateTimeOffset OccurredAt) : DomainEvent(OccurredAt);
