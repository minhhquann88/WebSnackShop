using MediatR;
using SnackShop.Api.Contracts;
using SnackShop.Api.Extensions;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Inventory;
using SnackShop.Application.Features.Inventory.AdjustStock;
using SnackShop.Application.Features.Inventory.ListStock;
using SnackShop.Application.Features.Inventory.ListStockMovements;
using SnackShop.Application.Features.Inventory.ReceiveStock;
using SnackShop.Application.Features.Inventory.SetLowStockThreshold;

namespace SnackShop.Api.Endpoints.Admin;

/// <summary>Stock levels and movements. Staff may view and receive goods; only Admin may adjust or change thresholds.</summary>
public sealed class AdminInventoryEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup(ApiRoutes.AdminV1 + "/inventory").WithTags("Admin · Inventory").RequireAuthorization(Policies.Staff);

        g.MapGet("/", async ([AsParameters] ListStockQuery q, ISender s, CancellationToken ct) => (await s.Send(q, ct)).ToOk())
            .WithName("AdminListStock").Produces<PagedResponse<StockLevelResponse>>().ProducesErrors().ProducesAuthErrors();

        g.MapGet("/{variantId:guid}/movements", async (Guid variantId, int? page, int? pageSize, ISender s, CancellationToken ct) =>
                (await s.Send(new ListStockMovementsQuery(variantId, page ?? 1, pageSize ?? PagedResponseExtensions.DefaultPageSize), ct)).ToOk())
            .WithName("AdminListStockMovements").Produces<PagedResponse<StockMovementResponse>>()
            .ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        g.MapPost("/{variantId:guid}/receive", async (Guid variantId, ReceiveStockRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new ReceiveStockCommand(variantId, r.Quantity, r.Note), ct)).ToNoContent())
            .WithName("AdminReceiveStock").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapPost("/{variantId:guid}/adjust", async (Guid variantId, AdjustStockRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new AdjustStockCommand(variantId, r.Delta, r.Reason), ct)).ToNoContent())
            .RequireAuthorization(Policies.Admin)
            .WithName("AdminAdjustStock").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapPut("/{variantId:guid}/threshold", async (Guid variantId, ThresholdRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new SetLowStockThresholdCommand(variantId, r.Threshold), ct)).ToNoContent())
            .RequireAuthorization(Policies.Admin)
            .WithName("AdminSetLowStockThreshold").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();
    }
}
