using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Categories.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryCommandHandler(ICategoryRepository categories)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var slugResult = await CategoryUniqueness.ResolveAsync(categories, command.Name, command.Slug, excludeId: null, cancellationToken);
        if (slugResult.IsFailure)
        {
            return Result.Failure<Guid>(slugResult.Error);
        }

        var category = Category.Create(command.Name, slugResult.Value, command.Description, command.ImageUrl, command.SortOrder);
        categories.Add(category);
        return category.Id;
    }
}
