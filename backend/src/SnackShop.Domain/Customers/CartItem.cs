using SnackShop.Domain.Common;

namespace SnackShop.Domain.Customers;

/// <summary>A variant and quantity in a cart. Prices are never stored here; they are read live.</summary>
public sealed class CartItem : Entity
{
    private CartItem()
    {
    }

    internal CartItem(Guid cartId, Guid variantId, int quantity, DateTimeOffset addedAt)
        : base(Guid.CreateVersion7())
    {
        CartId = cartId;
        VariantId = variantId;
        Quantity = quantity;
        AddedAt = addedAt;
    }

    public Guid CartId { get; private set; }

    public Guid VariantId { get; private set; }

    public int Quantity { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }

    internal void SetQuantity(int quantity) => Quantity = quantity;
}
