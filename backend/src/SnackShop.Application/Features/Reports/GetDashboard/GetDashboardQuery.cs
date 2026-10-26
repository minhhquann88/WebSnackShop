using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Reports.GetDashboard;

public sealed record GetDashboardQuery : IQuery<DashboardResponse>;
