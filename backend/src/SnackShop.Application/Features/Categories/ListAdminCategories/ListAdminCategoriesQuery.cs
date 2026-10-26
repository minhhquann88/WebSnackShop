using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Categories.ListAdminCategories;

/// <summary>All categories (active and hidden) for the admin, ordered by SortOrder.</summary>
public sealed record ListAdminCategoriesQuery : IQuery<IReadOnlyList<AdminCategoryResponse>>;
