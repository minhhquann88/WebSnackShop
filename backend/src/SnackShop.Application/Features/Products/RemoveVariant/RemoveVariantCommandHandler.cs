using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.RemoveVariant;

public sealed class RemoveVariantCommandHandler(IProductRepository products)
    : ICommandHandler<RemoveVariantCommand>
{
    public async Task<Result> Handle(RemoveVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        return product is null
            ? Result.Failure(CatalogErrors.ProductNotFound)
            : product.RemoveVariant(command.VariantId);
    }
}
