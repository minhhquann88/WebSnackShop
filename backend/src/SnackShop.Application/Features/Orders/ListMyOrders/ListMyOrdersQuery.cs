using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Orders.ListMyOrders;

/// <summary>Order history of the signed-in customer, newest first.</summary>
public sealed record ListMyOrdersQuery(
    int Page = 1,
    int PageSize = PagedResponseExtensions.DefaultPageSize) : IQuery<PagedResponse<OrderSummaryResponse>>, IPagedQuery;
