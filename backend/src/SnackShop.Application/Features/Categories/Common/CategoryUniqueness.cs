using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Categories.Common;

/// <summary>Builds the slug (explicit or from the name) and checks name/slug uniqueness across categories.</summary>
internal static class CategoryUniqueness
{
    public static async Task<Result<Slug>> ResolveAsync(
        ICategoryRepository categories,
        string name,
        string? requestedSlug,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var slugResult = string.IsNullOrWhiteSpace(requestedSlug) ? Slug.FromText(name) : Slug.Create(requestedSlug.Trim());
        if (slugResult.IsFailure)
        {
            return slugResult;
        }

        if (await categories.NameExistsAsync(name.Trim(), excludeId, cancellationToken))
        {
            return Result.Failure<Slug>(CatalogErrors.DuplicateCategoryName);
        }

        if (await categories.SlugExistsAsync(slugResult.Value, excludeId, cancellationToken))
        {
            return Result.Failure<Slug>(CatalogErrors.DuplicateCategorySlug);
        }

        return slugResult;
    }
}
