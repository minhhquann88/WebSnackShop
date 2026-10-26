using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.ListAdminOrders;

/// <summary>
/// Admin order list, newest first. <see cref="Search"/> matches the order number, phone or email.
/// <see cref="From"/>/<see cref="To"/> filter on PlacedAt (inclusive / exclusive).
/// </summary>
public sealed record ListAdminOrdersQuery(
    OrderStatus? Status = null,
    PaymentMethod? PaymentMethod = null,
    PaymentStatus? PaymentStatus = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Search = null,
    int Page = 1,
    int PageSize = PagedResponseExtensions.DefaultPageSize) : IQuery<PagedResponse<AdminOrderSummaryResponse>>, IPagedQuery;
