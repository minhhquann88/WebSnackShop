using SnackShop.Domain.Common;

namespace SnackShop.Domain.Ordering;

/// <summary>Append-only record of a status change.</summary>
public sealed class OrderStatusHistory : Entity
{
    public const int MaxNoteLength = 500;

    private OrderStatusHistory()
    {
    }

    internal OrderStatusHistory(Guid orderId, OrderStatus? fromStatus, OrderStatus toStatus, OrderActor actor, string? note, DateTimeOffset changedAt)
        : base(Guid.CreateVersion7())
    {
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ActorType = actor.Type;
        ChangedByUserId = actor.UserId;
        Note = note;
        ChangedAt = changedAt;
    }

    public Guid OrderId { get; private set; }

    public OrderStatus? FromStatus { get; private set; }

    public OrderStatus ToStatus { get; private set; }

    public ActorType ActorType { get; private set; }

    public Guid? ChangedByUserId { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }
}
