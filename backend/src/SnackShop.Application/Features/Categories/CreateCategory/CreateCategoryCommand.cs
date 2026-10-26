using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Categories.CreateCategory;

/// <summary>Creates a category. When <see cref="Slug"/> is empty it is generated from the name.</summary>
public sealed record CreateCategoryCommand(
    string Name,
    string? Slug,
    string? Description,
    string? ImageUrl,
    int SortOrder) : ICommand<Guid>;
