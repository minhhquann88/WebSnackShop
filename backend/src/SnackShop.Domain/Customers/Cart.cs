using SnackShop.Domain.Common;

namespace SnackShop.Domain.Customers;

/// <summary>
/// Server-side cart of a signed-in customer (one per customer). Guests keep their cart in the browser and
/// merge it on sign-in. Quantities per line are capped at <see cref="MaxQuantityPerItem"/>.
/// </summary>
public sealed class Cart : AggregateRoot
{
    public const int MaxQuantityPerItem = 99;
    public const int MaxItems = 50;

    private readonly List<CartItem> _items = [];

    private Cart()
    {
    }

    private Cart(Guid id, Guid customerId, DateTimeOffset now)
        : base(id)
    {
        CustomerId = customerId;
        UpdatedAt = now;
    }

    public Guid CustomerId { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public static Cart CreateFor(Guid customerId, DateTimeOffset now) => new(Guid.CreateVersion7(), customerId, now);

    /// <summary>Adds to the existing quantity of the variant (capped at the max), or creates a new line.</summary>
    public Result AddItem(Guid variantId, int quantity, DateTimeOffset now)
    {
        if (quantity is < 1 or > MaxQuantityPerItem)
        {
            return Result.Failure(CustomerErrors.InvalidCartQuantity);
        }

        var existing = _items.Find(i => i.VariantId == variantId);
        if (existing is not null)
        {
            existing.SetQuantity(Math.Min(MaxQuantityPerItem, existing.Quantity + quantity));
        }
        else
        {
            if (_items.Count >= MaxItems)
            {
                return Result.Failure(CustomerErrors.TooManyCartItems);
            }

            _items.Add(new CartItem(Id, variantId, quantity, now));
        }

        UpdatedAt = now;
        return Result.Success();
    }

    public Result SetQuantity(Guid variantId, int quantity, DateTimeOffset now)
    {
        if (quantity is < 1 or > MaxQuantityPerItem)
        {
            return Result.Failure(CustomerErrors.InvalidCartQuantity);
        }

        var item = _items.Find(i => i.VariantId == variantId);
        if (item is null)
        {
            return Result.Failure(CustomerErrors.CartItemNotFound);
        }

        item.SetQuantity(quantity);
        UpdatedAt = now;
        return Result.Success();
    }

    public Result RemoveItem(Guid variantId, DateTimeOffset now)
    {
        var item = _items.Find(i => i.VariantId == variantId);
        if (item is null)
        {
            return Result.Failure(CustomerErrors.CartItemNotFound);
        }

        _items.Remove(item);
        UpdatedAt = now;
        return Result.Success();
    }

    public void Clear(DateTimeOffset now)
    {
        _items.Clear();
        UpdatedAt = now;
    }

    /// <summary>Merges a guest cart: quantities of the same variant are summed and capped. Invalid lines are skipped.</summary>
    public void Merge(IEnumerable<(Guid VariantId, int Quantity)> lines, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(lines);

        foreach (var (variantId, quantity) in lines)
        {
            if (quantity is >= 1 and <= MaxQuantityPerItem)
            {
                _ = AddItem(variantId, quantity, now);
            }
        }

        UpdatedAt = now;
    }

    /// <summary>Removes the lines of variants that were just ordered (after checkout).</summary>
    public void RemoveVariants(IReadOnlyCollection<Guid> variantIds, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(variantIds);
        _items.RemoveAll(i => variantIds.Contains(i.VariantId));
        UpdatedAt = now;
    }
}
