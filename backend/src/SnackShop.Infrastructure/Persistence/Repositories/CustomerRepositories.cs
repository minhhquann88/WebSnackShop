using Microsoft.EntityFrameworkCore;
using SnackShop.Domain.Customers;

namespace SnackShop.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(ApplicationDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.CustomerSet.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Add(Customer customer) => db.CustomerSet.Add(customer);
}

internal sealed class CartRepository(ApplicationDbContext db) : ICartRepository
{
    public Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken) =>
        db.CartSet.Include(c => c.Items).FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

    public void Add(Cart cart) => db.CartSet.Add(cart);
}
