using SnackShop.Domain.Common;

namespace SnackShop.Domain.Ordering;

/// <summary>An ordered line. Snapshots names, SKU and price at order time so later catalog edits never change history.</summary>
public sealed class OrderItem : Entity
{
    public const int MaxProductNameLength = 200;
    public const int MaxVariantNameLength = 100;
    public const int MaxSkuLength = 50;
    public const int MaxImageUrlLength = 500;

    private OrderItem()
    {
    }

    internal OrderItem(Guid orderId, OrderLine line)
        : base(Guid.CreateVersion7())
    {
        OrderId = orderId;
        VariantId = line.VariantId;
        ProductId = line.ProductId;
        ProductName = line.ProductName;
        VariantName = line.VariantName;
        Sku = line.Sku;
        ImageUrl = line.ImageUrl;
        UnitPrice = line.UnitPrice;
        Quantity = line.Quantity;
        LineTotal = line.UnitPrice * line.Quantity;
    }

    public Guid OrderId { get; private set; }

    public Guid VariantId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = default!;

    public string VariantName { get; private set; } = default!;

    public string Sku { get; private set; } = default!;

    public string? ImageUrl { get; private set; }

    public Money UnitPrice { get; private set; } = default!;

    public int Quantity { get; private set; }

    public Money LineTotal { get; private set; } = default!;
}

/// <summary>Snapshot input for one order line, built by the application from live catalog data.</summary>
public sealed record OrderLine(
    Guid VariantId,
    Guid ProductId,
    string ProductName,
    string VariantName,
    string Sku,
    string? ImageUrl,
    Money UnitPrice,
    int Quantity);
