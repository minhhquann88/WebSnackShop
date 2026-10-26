namespace SnackShop.Domain.Ordering;

/// <summary>Write-side access to orders with items, history and payment loaded.</summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Order?> GetByNumberAsync(OrderNumber orderNumber, CancellationToken cancellationToken);

    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);


    /// <summary>Allocates the next order number from the database sequence.</summary>
    Task<OrderNumber> NextOrderNumberAsync(DateTimeOffset placedAt, CancellationToken cancellationToken);

    void Add(Order order);
}
