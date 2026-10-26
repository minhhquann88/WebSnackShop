namespace SnackShop.Domain.Ordering;

/// <summary>The person or process acting on an order, recorded in the status history.</summary>
public sealed record OrderActor(ActorType Type, Guid? UserId)
{
    public static readonly OrderActor System = new(ActorType.System, null);

    public static OrderActor Customer(Guid? userId) => new(ActorType.Customer, userId);

    public static OrderActor Staff(Guid userId) => new(ActorType.Staff, userId);
}
