using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Features.Products.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.UploadProductImage;

public sealed class UploadProductImageCommandHandler(IProductRepository products, IFileStorage storage)
    : ICommandHandler<UploadProductImageCommand, ProductImageResponse>
{
    private const string Folder = "products";

    public async Task<Result<ProductImageResponse>> Handle(UploadProductImageCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductImageResponse>(CatalogErrors.ProductNotFound);
        }

        if (product.Images.Count >= Product.MaxImages)
        {
            return Result.Failure<ProductImageResponse>(CatalogErrors.TooManyImages);
        }

        // Read into memory (max 5 MB, validated) so we can sniff the header and then store the full content.
        using var buffer = new MemoryStream();
        await command.Content.CopyToAsync(buffer, cancellationToken);
        var extension = ImageFormat.DetectExtension(buffer.GetBuffer().AsSpan(0, (int)Math.Min(buffer.Length, ImageFormat.HeaderLength)));
        if (extension is null || buffer.Length > ImageFormat.MaxBytes)
        {
            return Result.Failure<ProductImageResponse>(CatalogErrors.UnsupportedImage);
        }

        buffer.Position = 0;
        var url = await storage.SaveAsync(buffer, extension, Folder, cancellationToken);

        var added = product.AddImage(url, command.AltText);
        if (added.IsFailure)
        {
            await storage.DeleteAsync(url, cancellationToken);
            return Result.Failure<ProductImageResponse>(added.Error);
        }

        var image = added.Value;
        return new ProductImageResponse(image.Id, image.Url, image.AltText, image.SortOrder, image.IsPrimary);
    }
}
