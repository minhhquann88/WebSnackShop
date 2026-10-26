using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Products;

/// <summary>Product card on listing pages. Price is the cheapest active variant.</summary>
public sealed record ProductSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? ShortDescription,
    string? ImageUrl,
    MoneyResponse? Price,
    MoneyResponse? CompareAtPrice,
    int SoldCount,
    bool IsFeatured,
    bool InStock);

public sealed record ProductImageResponse(Guid Id, string Url, string? AltText, int SortOrder, bool IsPrimary);

public sealed record ProductCategoryResponse(Guid Id, string Name, string Slug);

/// <summary>Storefront variant. <see cref="AvailableQuantity"/> is what can be ordered right now.</summary>
public sealed record ProductVariantResponse(
    Guid Id,
    string Name,
    string Sku,
    MoneyResponse Price,
    MoneyResponse? CompareAtPrice,
    int? WeightGrams,
    int AvailableQuantity);

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? Origin,
    string? Brand,
    int? ShelfLifeDays,
    int SoldCount,
    ProductCategoryResponse Category,
    IReadOnlyList<ProductImageResponse> Images,
    IReadOnlyList<ProductVariantResponse> Variants);

/// <summary>Admin product list row.</summary>
public sealed record AdminProductSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid CategoryId,
    string CategoryName,
    string? ImageUrl,
    MoneyResponse? MinPrice,
    int VariantCount,
    int TotalAvailable,
    bool IsActive,
    bool IsFeatured,
    int SoldCount,
    DateTimeOffset? UpdatedAt);

/// <summary>Admin variant with stock figures.</summary>
public sealed record AdminVariantResponse(
    Guid Id,
    string Name,
    string Sku,
    MoneyResponse Price,
    MoneyResponse? CompareAtPrice,
    int? WeightGrams,
    int SortOrder,
    bool IsActive,
    int QuantityOnHand,
    int QuantityReserved,
    int LowStockThreshold);

public sealed record AdminProductDetailResponse(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? Origin,
    string? Brand,
    int? ShelfLifeDays,
    bool IsActive,
    bool IsFeatured,
    int SoldCount,
    IReadOnlyList<ProductImageResponse> Images,
    IReadOnlyList<AdminVariantResponse> Variants,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
