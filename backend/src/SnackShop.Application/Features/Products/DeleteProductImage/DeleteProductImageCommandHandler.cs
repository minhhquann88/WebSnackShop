using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.DeleteProductImage;

public sealed class DeleteProductImageCommandHandler(IProductRepository products, IFileStorage storage)
    : ICommandHandler<DeleteProductImageCommand>
{
    public async Task<Result> Handle(DeleteProductImageCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(CatalogErrors.ProductNotFound);
        }

        var removed = product.RemoveImage(command.ImageId);
        if (removed.IsFailure)
        {
            return removed;
        }

        // Deleting the file before commit is acceptable: an orphan DB row is worse than a missing file, and
        // the image is gone from the product either way once the unit of work commits.
        await storage.DeleteAsync(removed.Value, cancellationToken);
        return Result.Success();
    }
}
