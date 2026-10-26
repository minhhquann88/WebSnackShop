using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Categories.ListCategories;

public sealed class ListCategoriesQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.Slug.Value,
                c.Description,
                c.ImageUrl,
                c.SortOrder,
                db.Products.Count(p => p.CategoryId == c.Id && p.IsActive)))
            .ToListAsync(cancellationToken);

        return categories;
    }
}
