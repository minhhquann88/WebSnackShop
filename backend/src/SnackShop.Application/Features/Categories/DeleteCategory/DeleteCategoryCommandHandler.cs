using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(ICategoryRepository categories)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
        {
            return Result.Failure(CatalogErrors.CategoryNotFound);
        }

        if (await categories.HasProductsAsync(category.Id, cancellationToken))
        {
            return Result.Failure(CatalogErrors.CategoryHasProducts);
        }

        categories.Remove(category);
        return Result.Success();
    }
}
