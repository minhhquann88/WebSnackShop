using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.UpdateProduct;

/// <summary>Replaces the descriptive fields of a product. Variants, images and visibility have their own commands.</summary>
public sealed record UpdateProductCommand(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Slug,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? Origin,
    string? Brand,
    int? ShelfLifeDays) : ICommand;
