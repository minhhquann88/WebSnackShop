using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.DeleteProduct;

public sealed class DeleteProductCommandHandler(IProductRepository products)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(CatalogErrors.ProductNotFound);
        }

        products.Remove(product);
        return Result.Success();
    }
}
