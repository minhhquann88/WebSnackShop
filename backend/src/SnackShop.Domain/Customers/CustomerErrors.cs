using SnackShop.Domain.Common;

namespace SnackShop.Domain.Customers;

public static class CustomerErrors
{
    public static readonly Error NotFound = Error.NotFound("Customers.NotFound", "The customer profile was not found.");

    public static readonly Error AddressNotFound = Error.NotFound("Customers.AddressNotFound", "The address was not found.");

    public static readonly Error TooManyAddresses = Error.Validation(
        "Customers.TooManyAddresses", $"A customer can save at most {Customer.MaxAddresses} addresses.");

    public static readonly Error InvalidCartQuantity = Error.Validation(
        "Customers.InvalidCartQuantity", $"Quantity must be between 1 and {Cart.MaxQuantityPerItem}.");

    public static readonly Error TooManyCartItems = Error.Validation(
        "Customers.TooManyCartItems", $"A cart can contain at most {Cart.MaxItems} different products.");

    public static readonly Error CartItemNotFound = Error.NotFound("Customers.CartItemNotFound", "The item is not in the cart.");
}
