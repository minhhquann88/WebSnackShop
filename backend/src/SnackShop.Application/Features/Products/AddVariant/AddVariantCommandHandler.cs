using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Features.Products.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Products.AddVariant;

public sealed class AddVariantCommandHandler(
    IProductRepository products,
    IStockItemRepository stockItems,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<AddVariantCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<Guid>(CatalogErrors.ProductNotFound);
        }

        var spec = await ProductRules.BuildVariantAsync(products, command.Variant, null, cancellationToken);
        if (spec.IsFailure)
        {
            return Result.Failure<Guid>(spec.Error);
        }

        var added = product.AddVariant(spec.Value);
        if (added.IsFailure)
        {
            return Result.Failure<Guid>(added.Error);
        }

        var stock = StockItem.CreateFor(added.Value.Id);
        if (command.InitialStock > 0)
        {
            stock.Receive(command.InitialStock, "Initial stock", new StockContext(timeProvider.GetUtcNow(), currentUser.UserId));
        }

        stockItems.Add(stock);
        return added.Value.Id;
    }
}
