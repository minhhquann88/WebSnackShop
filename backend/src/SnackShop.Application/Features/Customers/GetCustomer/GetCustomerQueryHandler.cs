using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Me.Addresses;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Customers.GetCustomer;

public sealed class GetCustomerQueryHandler(IApplicationDbContext db) : IQueryHandler<GetCustomerQuery, CustomerDetailResponse>
{
    private const int RecentOrderCount = 10;

    public async Task<Result<CustomerDetailResponse>> Handle(GetCustomerQuery query, CancellationToken cancellationToken)
    {
        var customer = await db.Customers.AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.Email,
                c.Phone,
                c.CreatedAt,
                OrderCount = db.Orders.Count(o => o.CustomerId == c.Id),
                Spent = db.Orders.Where(o => o.CustomerId == c.Id && o.Status == OrderStatus.Delivered).Sum(o => (decimal?)o.Total.Amount) ?? 0m,
                Addresses = c.Addresses.OrderByDescending(a => a.IsDefault)
                    .Select(a => new SavedAddressResponse(a.Id, a.Label, a.FullName, a.Phone, a.Line1, a.Ward, a.District, a.Province, a.IsDefault))
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerDetailResponse>(CustomerErrors.NotFound);
        }

        var recent = await db.Orders.AsNoTracking()
            .Where(o => o.CustomerId == query.Id)
            .OrderByDescending(o => o.PlacedAt)
            .Take(RecentOrderCount)
            .Select(OrderProjections.Summary)
            .ToListAsync(cancellationToken);

        return new CustomerDetailResponse(customer.Id, customer.FullName, customer.Email, customer.Phone, customer.CreatedAt,
            customer.OrderCount, new MoneyResponse(customer.Spent, Money.DefaultCurrency), customer.Addresses, recent);
    }
}
