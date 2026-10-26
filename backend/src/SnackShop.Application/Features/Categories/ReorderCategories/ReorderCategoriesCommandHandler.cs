using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Categories.ReorderCategories;

public sealed class ReorderCategoriesCommandHandler(ICategoryRepository categories)
    : ICommandHandler<ReorderCategoriesCommand>
{
    public async Task<Result> Handle(ReorderCategoriesCommand command, CancellationToken cancellationToken)
    {
        var all = await categories.ListAllAsync(cancellationToken);
        var byId = all.ToDictionary(c => c.Id);

        if (command.CategoryIds.Any(id => !byId.ContainsKey(id)))
        {
            return Result.Failure(CatalogErrors.CategoryNotFound);
        }

        for (var i = 0; i < command.CategoryIds.Count; i++)
        {
            byId[command.CategoryIds[i]].MoveTo(i);
        }

        return Result.Success();
    }
}
