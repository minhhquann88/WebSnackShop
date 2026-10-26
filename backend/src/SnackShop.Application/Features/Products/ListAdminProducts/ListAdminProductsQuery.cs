using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Products.ListAdminProducts;

/// <summary>Admin product list including hidden products, newest first.</summary>
public sealed record ListAdminProductsQuery(
    string? Search = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = PagedResponseExtensions.DefaultPageSize) : IQuery<PagedResponse<AdminProductSummaryResponse>>, IPagedQuery;
