using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Slug,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    bool IsActive) : ICommand;
