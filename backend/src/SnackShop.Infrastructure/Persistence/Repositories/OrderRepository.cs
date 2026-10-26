using Microsoft.EntityFrameworkCore;
using SnackShop.Domain.Ordering;

namespace SnackShop.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    private IQueryable<Order> WithChildren => db.OrderSet
        .Include(o => o.Items)
        .Include(o => o.StatusHistory)
        .Include(o => o.Payment)
        .AsSplitQuery();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        WithChildren.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<Order?> GetByNumberAsync(OrderNumber orderNumber, CancellationToken cancellationToken) =>
        WithChildren.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

    public Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        WithChildren.FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task<OrderNumber> NextOrderNumberAsync(DateTimeOffset placedAt, CancellationToken cancellationToken)
    {
        var next = await db.Database
            .SqlQueryRaw<long>($"SELECT nextval('{ApplicationDbContext.OrderNumberSequence}') AS \"Value\"")
            .SingleAsync(cancellationToken);

        return OrderNumber.Create(placedAt, next);
    }

    public void Add(Order order) => db.OrderSet.Add(order);
}
