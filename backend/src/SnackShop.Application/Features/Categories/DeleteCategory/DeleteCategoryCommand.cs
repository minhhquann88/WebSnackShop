using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Categories.DeleteCategory;

/// <summary>Soft-deletes an empty category.</summary>
public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
