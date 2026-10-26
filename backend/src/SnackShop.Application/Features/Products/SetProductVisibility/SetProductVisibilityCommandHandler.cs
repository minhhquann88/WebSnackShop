using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.SetProductVisibility;

public sealed class SetProductVisibilityCommandHandler(IProductRepository products)
    : ICommandHandler<SetProductVisibilityCommand>
{
    public async Task<Result> Handle(SetProductVisibilityCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.Id, cancellationToken);
        return product is null
            ? Result.Failure(CatalogErrors.ProductNotFound)
            : product.SetVisibility(command.IsActive, command.IsFeatured);
    }
}
