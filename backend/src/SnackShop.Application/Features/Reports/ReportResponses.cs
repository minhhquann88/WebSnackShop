using SnackShop.Application.Common;
using SnackShop.Application.Features.Inventory;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Reports;

public sealed record OrderStatusCountResponse(OrderStatus Status, int Count);

/// <summary>Admin home page. Revenue counts delivered orders by delivery date (Vietnam calendar days).</summary>
public sealed record DashboardResponse(
    MoneyResponse RevenueToday,
    MoneyResponse RevenueLast7Days,
    MoneyResponse RevenueLast30Days,
    int OrdersPlacedToday,
    int PendingOrders,
    int UnpaidBankTransfers,
    IReadOnlyList<OrderStatusCountResponse> OrdersByStatus,
    int LowStockCount,
    IReadOnlyList<StockLevelResponse> LowStockItems);

/// <summary>Revenue of one Vietnam calendar day.</summary>
public sealed record RevenuePointResponse(DateOnly Date, int OrderCount, MoneyResponse Revenue);

public sealed record RevenueReportResponse(
    DateOnly From,
    DateOnly To,
    int OrderCount,
    MoneyResponse TotalRevenue,
    IReadOnlyList<RevenuePointResponse> Days);

public sealed record TopProductResponse(Guid ProductId, string ProductName, int QuantitySold, MoneyResponse Revenue);
