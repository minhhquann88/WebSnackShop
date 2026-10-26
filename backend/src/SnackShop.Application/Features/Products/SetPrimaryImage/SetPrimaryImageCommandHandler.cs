using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.SetPrimaryImage;

public sealed class SetPrimaryImageCommandHandler(IProductRepository products)
    : ICommandHandler<SetPrimaryImageCommand>
{
    public async Task<Result> Handle(SetPrimaryImageCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        return product is null
            ? Result.Failure(CatalogErrors.ProductNotFound)
            : product.SetPrimaryImage(command.ImageId);
    }
}
