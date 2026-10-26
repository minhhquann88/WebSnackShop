using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Customers.GetCustomer;

/// <summary>Customer profile with address book and the 10 most recent orders.</summary>
public sealed record GetCustomerQuery(Guid Id) : IQuery<CustomerDetailResponse>;
