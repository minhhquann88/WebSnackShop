using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Categories.ReorderCategories;

/// <summary>Sets SortOrder of the listed categories to their position in the list.</summary>
public sealed record ReorderCategoriesCommand(IReadOnlyList<Guid> CategoryIds) : ICommand;
