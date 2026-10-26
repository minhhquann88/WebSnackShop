using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Categories.ListAdminCategories;

public sealed class ListAdminCategoriesQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListAdminCategoriesQuery, IReadOnlyList<AdminCategoryResponse>>
{
    public async Task<Result<IReadOnlyList<AdminCategoryResponse>>> Handle(ListAdminCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new AdminCategoryResponse(
                c.Id,
                c.Name,
                c.Slug.Value,
                c.Description,
                c.ImageUrl,
                c.SortOrder,
                c.IsActive,
                db.Products.Count(p => p.CategoryId == c.Id),
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return categories;
    }
}
