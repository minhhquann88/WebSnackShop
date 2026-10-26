using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Application.Features.Products.CreateProduct;

/// <summary>Creates a product with its first variant and that variant's stock record (with optional initial stock).</summary>
public sealed record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string? Slug,
    string? ShortDescription,
    string? Description,
    string? Ingredients,
    string? Origin,
    string? Brand,
    int? ShelfLifeDays,
    bool IsActive,
    bool IsFeatured,
    VariantRequest FirstVariant,
    int InitialStock = 0) : ICommand<Guid>;
