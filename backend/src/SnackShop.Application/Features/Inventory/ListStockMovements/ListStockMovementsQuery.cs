using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Inventory.ListStockMovements;

/// <summary>Stock history of one variant, newest first.</summary>
public sealed record ListStockMovementsQuery(
    Guid VariantId,
    int Page = 1,
    int PageSize = PagedResponseExtensions.DefaultPageSize) : IQuery<PagedResponse<StockMovementResponse>>, IPagedQuery;
