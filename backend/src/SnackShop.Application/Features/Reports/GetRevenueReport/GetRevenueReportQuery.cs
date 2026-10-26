using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Reports.GetRevenueReport;

/// <summary>Daily revenue of delivered orders between two Vietnam calendar dates (both inclusive), max 366 days.</summary>
public sealed record GetRevenueReportQuery(DateOnly From, DateOnly To) : IQuery<RevenueReportResponse>;
