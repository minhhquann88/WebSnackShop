using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Categories.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(ICategoryRepository categories)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
        {
            return Result.Failure(CatalogErrors.CategoryNotFound);
        }

        var slugResult = await CategoryUniqueness.ResolveAsync(categories, command.Name, command.Slug, category.Id, cancellationToken);
        if (slugResult.IsFailure)
        {
            return slugResult;
        }

        category.Update(command.Name, slugResult.Value, command.Description, command.ImageUrl, command.SortOrder, command.IsActive);
        return Result.Success();
    }
}
