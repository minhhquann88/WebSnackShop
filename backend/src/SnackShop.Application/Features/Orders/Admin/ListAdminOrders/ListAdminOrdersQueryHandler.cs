using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.ListAdminOrders;

public sealed class ListAdminOrdersQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListAdminOrdersQuery, PagedResponse<AdminOrderSummaryResponse>>
{
    public async Task<Result<PagedResponse<AdminOrderSummaryResponse>>> Handle(ListAdminOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = db.Orders.AsNoTracking();

        if (query.Status is { } status)
        {
            orders = orders.Where(o => o.Status == status);
        }

        if (query.PaymentMethod is { } method)
        {
            orders = orders.Where(o => o.PaymentMethod == method);
        }

        if (query.PaymentStatus is { } paymentStatus)
        {
            orders = orders.Where(o => o.Payment.Status == paymentStatus);
        }

        // timestamptz parameters must be UTC (Npgsql rejects other offsets).
        if (query.From?.ToUniversalTime() is { } from)
        {
            orders = orders.Where(o => o.PlacedAt >= from);
        }

        if (query.To?.ToUniversalTime() is { } to)
        {
            orders = orders.Where(o => o.PlacedAt < to);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            var number = OrderNumber.FromTrusted(term);
            var email = term.ToLowerInvariant();
            orders = orders.Where(o => o.OrderNumber == number || o.ShippingAddress.Phone == term || o.ContactEmail == email);
        }

        var page = await orders
            .OrderByDescending(o => o.PlacedAt)
            .Select(o => new AdminOrderSummaryResponse(
                o.Id,
                o.OrderNumber.Value,
                o.Status,
                o.PaymentMethod,
                o.Payment.Status,
                new MoneyResponse(o.Total.Amount, o.Total.Currency),
                o.Items.Sum(i => i.Quantity),
                o.ShippingAddress.FullName,
                o.ShippingAddress.Phone,
                o.ContactEmail,
                o.CustomerId,
                o.PlacedAt))
            .ToPagedResponseAsync(query.Page, query.PageSize, cancellationToken);

        return page;
    }
}
