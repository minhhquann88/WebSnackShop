namespace SnackShop.Domain.Common;

/// <summary>
/// Something that happened in the domain. Raised by aggregates, dispatched after the
/// transaction commits (see docs/02-architecture.md §6).
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }
}

/// <summary>Convenience base for domain events.</summary>
public abstract record DomainEvent(DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
}
