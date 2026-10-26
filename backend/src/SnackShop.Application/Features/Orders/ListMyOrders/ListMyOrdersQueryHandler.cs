using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.ListMyOrders;

public sealed class ListMyOrdersQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IQueryHandler<ListMyOrdersQuery, PagedResponse<OrderSummaryResponse>>
{
    public async Task<Result<PagedResponse<OrderSummaryResponse>>> Handle(ListMyOrdersQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        if (userId.IsFailure)
        {
            return Result.Failure<PagedResponse<OrderSummaryResponse>>(userId.Error);
        }

        var page = await db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == userId.Value)
            .OrderByDescending(o => o.PlacedAt)
            .Select(OrderProjections.Summary)
            .ToPagedResponseAsync(query.Page, query.PageSize, cancellationToken);

        return page;
    }
}
