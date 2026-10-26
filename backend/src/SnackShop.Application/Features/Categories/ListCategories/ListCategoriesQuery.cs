using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Categories.ListCategories;

/// <summary>Active categories for the storefront menu, ordered by SortOrder.</summary>
public sealed record ListCategoriesQuery : IQuery<IReadOnlyList<CategoryResponse>>;
