using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.ReorderImages;

/// <summary>Sets the display order of a product's images. Must list every image exactly once.</summary>
public sealed record ReorderImagesCommand(Guid ProductId, IReadOnlyList<Guid> ImageIds) : ICommand;
