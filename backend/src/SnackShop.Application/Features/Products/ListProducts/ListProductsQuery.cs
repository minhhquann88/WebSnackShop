using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Products.ListProducts;

/// <summary>
/// Storefront product listing. <see cref="Sort"/>: createdAt, price, soldCount, name; prefix "-" for descending.
/// Default "-createdAt". <see cref="Search"/> is accent-insensitive.
/// </summary>
public sealed record ListProductsQuery(
    string? CategorySlug = null,
    string? Search = null,
    string? Sort = null,
    bool? Featured = null,
    int Page = 1,
    int PageSize = PagedResponseExtensions.DefaultPageSize) : IQuery<PagedResponse<ProductSummaryResponse>>, IPagedQuery
{
    public static readonly IReadOnlyList<string> AllowedSorts =
        ["createdAt", "-createdAt", "price", "-price", "soldCount", "-soldCount", "name", "-name"];
}
