using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Inventory.ListStock;

/// <summary>Stock per variant of non-deleted products, lowest availability first.</summary>
public sealed record ListStockQuery(
    string? Search = null,
    bool LowStockOnly = false,
    int Page = 1,
    int PageSize = PagedResponseExtensions.DefaultPageSize) : IQuery<PagedResponse<StockLevelResponse>>, IPagedQuery;
