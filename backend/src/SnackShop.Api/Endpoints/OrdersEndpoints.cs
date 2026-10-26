using MediatR;
using Microsoft.AspNetCore.Mvc;
using SnackShop.Api.Contracts;
using SnackShop.Api.Extensions;
using SnackShop.Application.Features.Orders;
using SnackShop.Application.Features.Orders.EstimateOrder;
using SnackShop.Application.Features.Orders.PlaceOrder;
using SnackShop.Application.Features.Orders.TrackOrder;

namespace SnackShop.Api.Endpoints;

/// <summary>Checkout and guest tracking. Works for guests; a bearer token attaches the order to the customer.</summary>
public sealed class OrdersEndpoints : IEndpointGroup
{
    public const string IdempotencyHeader = "Idempotency-Key";

    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.V1 + "/orders").WithTags("Orders").AllowAnonymous();

        group.MapPost("/estimate", EstimateAsync)
            .WithName("EstimateOrder")
            .WithSummary("Price a prospective order with live prices, stock and shipping fee.")
            .Produces<OrderEstimateResponse>()
            .ProducesErrors();

        group.MapPost("/", PlaceAsync)
            .WithName("PlaceOrder")
            .WithSummary("Place an order (COD or bank transfer). Send an Idempotency-Key header to make retries safe.")
            .RequireRateLimiting(RateLimitPolicies.PlaceOrder)
            .Produces<PlaceOrderResponse>(StatusCodes.Status201Created)
            .ProducesErrors(StatusCodes.Status409Conflict, StatusCodes.Status429TooManyRequests);

        group.MapGet("/track", TrackAsync)
            .WithName("TrackOrder")
            .WithSummary("Guest order tracking by order number and delivery phone.")
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .Produces<OrderDetailResponse>()
            .ProducesErrors(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> EstimateAsync(EstimateOrderQuery query, ISender sender, CancellationToken ct) =>
        (await sender.Send(query, ct)).ToOk();

    private static async Task<IResult> PlaceAsync(
        PlaceOrderRequest request,
        [FromHeader(Name = IdempotencyHeader)] string? idempotencyKey,
        ISender sender,
        CancellationToken ct)
    {
        var command = new PlaceOrderCommand(request.Items, request.ContactEmail, request.ShippingAddress,
            request.PaymentMethod, request.CustomerNote, idempotencyKey);
        var result = await sender.Send(command, ct);
        return result.ToCreated(r => $"{ApiRoutes.V1}/orders/track?orderNumber={r.OrderNumber}");
    }

    private static async Task<IResult> TrackAsync(string orderNumber, string phone, ISender sender, CancellationToken ct) =>
        (await sender.Send(new TrackOrderQuery(orderNumber, phone), ct)).ToOk();
}
