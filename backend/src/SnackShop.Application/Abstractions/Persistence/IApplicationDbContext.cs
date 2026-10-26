using SnackShop.Domain.Catalog;
using SnackShop.Domain.Customers;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;
using SnackShop.Domain.Settings;

namespace SnackShop.Application.Abstractions.Persistence;

/// <summary>
/// Read-side access for query handlers. Exposes <see cref="IQueryable{T}"/> only: queries project with
/// Select and never add/remove entities. Commands go through repositories.
/// Soft-deleted catalog rows are already filtered out.
/// </summary>
public interface IApplicationDbContext
{
    IQueryable<ShopSettings> ShopSettings { get; }

    IQueryable<Category> Categories { get; }

    IQueryable<Product> Products { get; }

    IQueryable<StockItem> StockItems { get; }

    IQueryable<StockMovement> StockMovements { get; }

    IQueryable<Customer> Customers { get; }

    IQueryable<Cart> Carts { get; }

    IQueryable<Order> Orders { get; }
}
