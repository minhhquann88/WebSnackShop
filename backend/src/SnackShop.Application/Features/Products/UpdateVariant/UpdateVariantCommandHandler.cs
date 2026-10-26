using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Products.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.UpdateVariant;

public sealed class UpdateVariantCommandHandler(IProductRepository products)
    : ICommandHandler<UpdateVariantCommand>
{
    public async Task<Result> Handle(UpdateVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(CatalogErrors.ProductNotFound);
        }

        var spec = await ProductRules.BuildVariantAsync(products, command.Variant, command.VariantId, cancellationToken);
        if (spec.IsFailure)
        {
            return spec;
        }

        return product.UpdateVariant(command.VariantId, spec.Value, command.IsActive);
    }
}
