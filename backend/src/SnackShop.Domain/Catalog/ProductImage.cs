using SnackShop.Domain.Common;

namespace SnackShop.Domain.Catalog;

/// <summary>An image of a product. Exactly one image is primary whenever the product has images.</summary>
public sealed class ProductImage : Entity
{
    public const int MaxUrlLength = 500;
    public const int MaxAltTextLength = 200;

    private ProductImage()
    {
    }

    internal ProductImage(Guid productId, string url, string? altText, int sortOrder, bool isPrimary)
        : base(Guid.CreateVersion7())
    {
        ProductId = productId;
        Url = url;
        AltText = altText;
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
    }

    public Guid ProductId { get; private set; }

    public string Url { get; private set; } = default!;

    public string? AltText { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsPrimary { get; private set; }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;

    internal void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
