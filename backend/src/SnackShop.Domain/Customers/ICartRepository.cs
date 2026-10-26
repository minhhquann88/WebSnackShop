namespace SnackShop.Domain.Customers;

/// <summary>Write-side access to carts (one per customer).</summary>
public interface ICartRepository
{
    Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);

    void Add(Cart cart);
}
