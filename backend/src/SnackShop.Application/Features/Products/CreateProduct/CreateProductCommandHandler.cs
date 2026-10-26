using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Features.Products.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Products.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IStockItemRepository stockItems,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        if (!await categories.ExistsAsync(command.CategoryId, cancellationToken))
        {
            return Result.Failure<Guid>(CatalogErrors.CategoryNotFound);
        }

        var slug = await ProductRules.ResolveSlugAsync(products, command.Name, command.Slug, null, cancellationToken);
        var variant = await ProductRules.BuildVariantAsync(products, command.FirstVariant, null, cancellationToken);
        if (slug.IsFailure || variant.IsFailure)
        {
            return Result.Failure<Guid>(slug.IsFailure ? slug.Error : variant.Error);
        }

        var details = new ProductDetails(command.Name, command.ShortDescription, command.Description,
            command.Ingredients, command.Origin, command.Brand, command.ShelfLifeDays);
        var created = Product.Create(command.CategoryId, details, slug.Value, variant.Value);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        var product = created.Value;
        var visibility = product.SetVisibility(command.IsActive, command.IsFeatured);
        if (visibility.IsFailure)
        {
            return Result.Failure<Guid>(visibility.Error);
        }

        var stock = StockItem.CreateFor(product.Variants.Single().Id);
        if (command.InitialStock > 0)
        {
            stock.Receive(command.InitialStock, "Initial stock", new StockContext(timeProvider.GetUtcNow(), currentUser.UserId));
        }

        products.Add(product);
        stockItems.Add(stock);
        return product.Id;
    }
}
