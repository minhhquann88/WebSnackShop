using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.Common;

/// <summary>Cross-aggregate checks for product commands (uniqueness lives in the database, not the aggregate).</summary>
internal static class ProductRules
{
    public static async Task<Result<Slug>> ResolveSlugAsync(
        IProductRepository products,
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

        return await products.SlugExistsAsync(slugResult.Value, excludeId, cancellationToken)
            ? Result.Failure<Slug>(CatalogErrors.DuplicateProductSlug)
            : slugResult;
    }

    public static async Task<Result<VariantSpec>> BuildVariantAsync(
        IProductRepository products,
        VariantRequest request,
        Guid? excludeVariantId,
        CancellationToken cancellationToken)
    {
        var spec = request.ToSpec();
        if (spec.IsFailure)
        {
            return spec;
        }

        return await products.SkuExistsAsync(spec.Value.Sku, excludeVariantId, cancellationToken)
            ? Result.Failure<VariantSpec>(CatalogErrors.DuplicateSku)
            : spec;
    }
}
