using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Customers.ListCustomers;

/// <summary>Registered customers, newest first. <see cref="Search"/> matches name, email or phone.</summary>
public sealed record ListCustomersQuery(
    string? Search = null,
    int Page = 1,
    int PageSize = PagedResponseExtensions.DefaultPageSize) : IQuery<PagedResponse<CustomerSummaryResponse>>, IPagedQuery;
