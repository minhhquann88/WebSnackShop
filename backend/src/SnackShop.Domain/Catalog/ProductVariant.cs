using SnackShop.Domain.Common;

namespace SnackShop.Domain.Catalog;

/// <summary>A purchasable version of a product (flavour / weight). Owns the price and SKU. Part of the Product aggregate.</summary>
public sealed class ProductVariant : Entity
{
    public const int MaxNameLength = 100;
    public const string DefaultName = "Default";

    private ProductVariant()
    {
    }

    internal ProductVariant(Guid productId, string name, Sku sku, Money price, Money? compareAtPrice, int? weightGrams, int sortOrder)
        : base(Guid.CreateVersion7())
    {
        ProductId = productId;
        Name = name;
        Sku = sku;
        Price = price;
        CompareAtPrice = compareAtPrice;
        WeightGrams = weightGrams;
        SortOrder = sortOrder;
        IsActive = true;
    }

    public Guid ProductId { get; private set; }

    public string Name { get; private set; } = default!;

    public Sku Sku { get; private set; } = default!;

    public Money Price { get; private set; } = default!;

    public Money? CompareAtPrice { get; private set; }

    public int? WeightGrams { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    internal void Update(string name, Sku sku, Money price, Money? compareAtPrice, int? weightGrams, int sortOrder)
    {
        Name = name;
        Sku = sku;
        Price = price;
        CompareAtPrice = compareAtPrice;
        WeightGrams = weightGrams;
        SortOrder = sortOrder;
    }

    internal void SetActive(bool isActive) => IsActive = isActive;

    internal static Result ValidatePricing(Money price, Money? compareAtPrice)
    {
        if (price.IsZero)
        {
            return Result.Failure(CatalogErrors.InvalidPrice);
        }

        if (compareAtPrice is not null && compareAtPrice <= price)
        {
            return Result.Failure(CatalogErrors.InvalidCompareAtPrice);
        }

        return Result.Success();
    }
}
