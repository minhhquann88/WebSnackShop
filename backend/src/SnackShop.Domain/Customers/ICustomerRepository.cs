namespace SnackShop.Domain.Customers;

/// <summary>Write-side access to customers with their address book.</summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Customer customer);
}
