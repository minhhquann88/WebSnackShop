using SnackShop.Application.Common;
using SnackShop.Application.Common.Catalog;

namespace SnackShop.Application.Features.Cart;

/// <summary>A cart or estimate line priced from live catalog data.</summary>
public sealed record PricedLineResponse(
    Guid VariantId,
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    string VariantName,
    string Sku,
    string? ImageUrl,
    MoneyResponse UnitPrice,
    MoneyResponse? CompareAtPrice,
    int Quantity,
    MoneyResponse LineTotal,
    int Available,
    bool IsAvailable)
{
    public static PricedLineResponse From(VariantSnapshot v, int quantity) => new(
        v.VariantId,
        v.ProductId,
        v.ProductName,
        v.ProductSlug,
        v.VariantName,
        v.Sku,
        v.ImageUrl,
        MoneyResponse.From(v.Price),
        MoneyResponse.FromNullable(v.CompareAtPrice),
        quantity,
        MoneyResponse.From(v.Price * quantity),
        v.Available,
        v.IsPurchasable && v.Available >= quantity);
}

public sealed record CartResponse(
    IReadOnlyList<PricedLineResponse> Items,
    int ItemCount,
    MoneyResponse Subtotal,
    DateTimeOffset? UpdatedAt);

/// <summary>Input line used by guest cart merge, estimate and checkout.</summary>
public sealed record CartLineRequest(Guid VariantId, int Quantity);
