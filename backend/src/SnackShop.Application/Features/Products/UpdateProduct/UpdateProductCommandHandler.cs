using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Products.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.UpdateProduct;

public sealed class UpdateProductCommandHandler(IProductRepository products, ICategoryRepository categories)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(CatalogErrors.ProductNotFound);
        }

        if (product.CategoryId != command.CategoryId && !await categories.ExistsAsync(command.CategoryId, cancellationToken))
        {
            return Result.Failure(CatalogErrors.CategoryNotFound);
        }

        var slug = await ProductRules.ResolveSlugAsync(products, command.Name, command.Slug, product.Id, cancellationToken);
        if (slug.IsFailure)
        {
            return slug;
        }

        var details = new ProductDetails(command.Name, command.ShortDescription, command.Description,
            command.Ingredients, command.Origin, command.Brand, command.ShelfLifeDays);
        product.UpdateDetails(command.CategoryId, details, slug.Value);
        return Result.Success();
    }
}
