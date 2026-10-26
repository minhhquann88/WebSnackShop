using MediatR;
using SnackShop.Api.Extensions;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Reports;
using SnackShop.Application.Features.Reports.GetDashboard;
using SnackShop.Application.Features.Reports.GetRevenueReport;
using SnackShop.Application.Features.Reports.GetTopProducts;

namespace SnackShop.Api.Endpoints.Admin;

/// <summary>Dashboard (Staff) and sales reports (Admin). Dates are Vietnam calendar days (yyyy-MM-dd).</summary>
public sealed class AdminReportsEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup(ApiRoutes.AdminV1 + "/reports").WithTags("Admin · Reports").RequireAuthorization(Policies.Staff);

        g.MapGet("/dashboard", async (ISender s, CancellationToken ct) => (await s.Send(new GetDashboardQuery(), ct)).ToOk())
            .WithName("AdminGetDashboard").Produces<DashboardResponse>().ProducesAuthErrors();

        g.MapGet("/revenue", async (DateOnly from, DateOnly to, ISender s, CancellationToken ct) =>
                (await s.Send(new GetRevenueReportQuery(from, to), ct)).ToOk())
            .RequireAuthorization(Policies.Admin)
            .WithName("AdminGetRevenueReport").Produces<RevenueReportResponse>().ProducesErrors().ProducesAuthErrors();

        g.MapGet("/top-products", async (DateOnly from, DateOnly to, int? limit, ISender s, CancellationToken ct) =>
                (await s.Send(new GetTopProductsQuery(from, to, limit ?? 10), ct)).ToOk())
            .RequireAuthorization(Policies.Admin)
            .WithName("AdminGetTopProducts").Produces<IReadOnlyList<TopProductResponse>>().ProducesErrors().ProducesAuthErrors();
    }
}
