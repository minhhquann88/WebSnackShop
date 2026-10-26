using MediatR;
using SnackShop.Api.Contracts;
using SnackShop.Api.Extensions;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Orders;
using SnackShop.Application.Features.Orders.Admin.CancelOrder;
using SnackShop.Application.Features.Orders.Admin.ConfirmOrder;
using SnackShop.Application.Features.Orders.Admin.DeliverOrder;
using SnackShop.Application.Features.Orders.Admin.GetAdminOrder;
using SnackShop.Application.Features.Orders.Admin.ListAdminOrders;
using SnackShop.Application.Features.Orders.Admin.MarkOrderPaid;
using SnackShop.Application.Features.Orders.Admin.MarkOrderRefunded;
using SnackShop.Application.Features.Orders.Admin.ShipOrder;
using SnackShop.Application.Features.Orders.Admin.StartPreparingOrder;
using SnackShop.Application.Features.Orders.Admin.UpdateInternalNote;

namespace SnackShop.Api.Endpoints.Admin;

/// <summary>Order processing for staff. Each transition is its own action (docs/05 §1).</summary>
public sealed class AdminOrdersEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var g = app.MapGroup(ApiRoutes.AdminV1 + "/orders").WithTags("Admin · Orders").RequireAuthorization(Policies.Staff);

        g.MapGet("/", async ([AsParameters] ListAdminOrdersQuery q, ISender s, CancellationToken ct) => (await s.Send(q, ct)).ToOk())
            .WithName("AdminListOrders").Produces<PagedResponse<AdminOrderSummaryResponse>>().ProducesErrors().ProducesAuthErrors();

        g.MapGet("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new GetAdminOrderQuery(id), ct)).ToOk())
            .WithName("AdminGetOrder").Produces<AdminOrderDetailResponse>().ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        Action(g, "/{id:guid}/confirm", "AdminConfirmOrder", "Confirm a pending order (commits stock; bank transfers must be paid).",
            async (Guid id, ConfirmOrderRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new ConfirmOrderCommand(id, r.MarkPaid, r.TransactionRef), ct)).ToNoContent());

        Action(g, "/{id:guid}/prepare", "AdminStartPreparingOrder", "Mark a confirmed order as being prepared.",
            async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new StartPreparingOrderCommand(id), ct)).ToNoContent());

        Action(g, "/{id:guid}/ship", "AdminShipOrder", "Hand the order to the shipper.",
            async (Guid id, ShipOrderRequest r, ISender s, CancellationToken ct) => (await s.Send(new ShipOrderCommand(id, r.Note), ct)).ToNoContent());

        Action(g, "/{id:guid}/deliver", "AdminDeliverOrder", "Mark the order delivered (COD becomes paid).",
            async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeliverOrderCommand(id), ct)).ToNoContent());

        Action(g, "/{id:guid}/cancel", "AdminCancelOrder", "Cancel the order with a reason (stock released or returned).",
            async (Guid id, CancelRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new CancelOrderCommand(id, r.Reason ?? string.Empty), ct)).ToNoContent());

        Action(g, "/{id:guid}/payment/mark-paid", "AdminMarkOrderPaid", "Record that payment was received.",
            async (Guid id, MarkPaidRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new MarkOrderPaidCommand(id, r.TransactionRef, r.PaidAt), ct)).ToNoContent());

        Action(g, "/{id:guid}/payment/mark-refunded", "AdminMarkOrderRefunded", "Confirm the money of a cancelled paid order was returned.",
            async (Guid id, RefundRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new MarkOrderRefundedCommand(id, r.TransactionRef), ct)).ToNoContent());

        g.MapPut("/{id:guid}/internal-note", async (Guid id, InternalNoteRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new UpdateInternalNoteCommand(id, r.Note), ct)).ToNoContent())
            .WithName("AdminUpdateInternalNote").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();
    }

    private static void Action(RouteGroupBuilder g, string pattern, string name, string summary, Delegate handler) =>
        g.MapPost(pattern, handler)
            .WithName(name)
            .WithSummary(summary)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict)
            .ProducesAuthErrors();
}
