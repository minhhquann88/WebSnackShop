using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Customers.ListCustomers;

public sealed class ListCustomersQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListCustomersQuery, PagedResponse<CustomerSummaryResponse>>
{
    public async Task<Result<PagedResponse<CustomerSummaryResponse>>> Handle(ListCustomersQuery query, CancellationToken cancellationToken)
    {
        var customers = db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
#pragma warning disable CA1304, CA1311, CA1862 // Translated to SQL lower(); .NET culture rules do not apply.
            customers = customers.Where(c => c.FullName.ToLower().Contains(term) || c.Email.Contains(term) || c.Phone == term);
#pragma warning restore CA1304, CA1311, CA1862
        }

        var page = await customers
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerSummaryResponse(
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                db.Orders.Count(o => o.CustomerId == c.Id),
                new MoneyResponse(
                    db.Orders.Where(o => o.CustomerId == c.Id && o.Status == OrderStatus.Delivered).Sum(o => (decimal?)o.Total.Amount) ?? 0m,
                    Money.DefaultCurrency),
                db.Orders.Where(o => o.CustomerId == c.Id).Max(o => (DateTimeOffset?)o.PlacedAt),
                c.CreatedAt))
            .ToPagedResponseAsync(query.Page, query.PageSize, cancellationToken);

        return page;
    }
}
