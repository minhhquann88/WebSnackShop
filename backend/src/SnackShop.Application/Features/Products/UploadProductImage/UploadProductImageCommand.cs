using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.UploadProductImage;

/// <summary>Stores an uploaded image and attaches it to the product. The first image becomes primary.</summary>
public sealed record UploadProductImageCommand(
    Guid ProductId,
    Stream Content,
    long Length,
    string? AltText) : ICommand<ProductImageResponse>;
