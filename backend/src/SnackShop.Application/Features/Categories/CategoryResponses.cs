namespace SnackShop.Application.Features.Categories;

/// <summary>Storefront category with the number of visible products.</summary>
public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    int ProductCount);

/// <summary>Admin view of a category, including hidden ones.</summary>
public sealed record AdminCategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    int ProductCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
