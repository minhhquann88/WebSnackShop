using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.ReorderImages;

public sealed class ReorderImagesCommandHandler(IProductRepository products)
    : ICommandHandler<ReorderImagesCommand>
{
    public async Task<Result> Handle(ReorderImagesCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        return product is null
            ? Result.Failure(CatalogErrors.ProductNotFound)
            : product.ReorderImages(command.ImageIds);
    }
}
