using SnackShop.Application.Common;
using SnackShop.Application.Features.Me.Addresses;
using SnackShop.Application.Features.Orders;

namespace SnackShop.Application.Features.Customers;

/// <summary>Admin customer list row. Spend counts delivered orders only.</summary>
public sealed record CustomerSummaryResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    int OrderCount,
    MoneyResponse TotalSpent,
    DateTimeOffset? LastOrderAt,
    DateTimeOffset CreatedAt);

public sealed record CustomerDetailResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    DateTimeOffset CreatedAt,
    int OrderCount,
    MoneyResponse TotalSpent,
    IReadOnlyList<SavedAddressResponse> Addresses,
    IReadOnlyList<OrderSummaryResponse> RecentOrders);
