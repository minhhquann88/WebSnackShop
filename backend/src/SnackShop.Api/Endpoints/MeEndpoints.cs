using MediatR;
using SnackShop.Api.Contracts;
using SnackShop.Api.Extensions;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Auth;
using SnackShop.Application.Features.Cart;
using SnackShop.Application.Features.Cart.AddCartItem;
using SnackShop.Application.Features.Cart.ClearCart;
using SnackShop.Application.Features.Cart.GetMyCart;
using SnackShop.Application.Features.Cart.MergeCart;
using SnackShop.Application.Features.Cart.RemoveCartItem;
using SnackShop.Application.Features.Cart.UpdateCartItem;
using SnackShop.Application.Features.Me.Addresses;
using SnackShop.Application.Features.Me.Addresses.AddMyAddress;
using SnackShop.Application.Features.Me.Addresses.DeleteMyAddress;
using SnackShop.Application.Features.Me.Addresses.ListMyAddresses;
using SnackShop.Application.Features.Me.Addresses.SetDefaultAddress;
using SnackShop.Application.Features.Me.Addresses.UpdateMyAddress;
using SnackShop.Application.Features.Me.ChangePassword;
using SnackShop.Application.Features.Me.GetMe;
using SnackShop.Application.Features.Me.UpdateProfile;
using SnackShop.Application.Features.Orders;
using SnackShop.Application.Features.Orders.CancelMyOrder;
using SnackShop.Application.Features.Orders.GetMyOrder;
using SnackShop.Application.Features.Orders.ListMyOrders;

namespace SnackShop.Api.Endpoints;

/// <summary>The signed-in user's own data. Never accepts a user id from the client.</summary>
public sealed class MeEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var me = app.MapGroup(ApiRoutes.MeV1).WithTags("Me").RequireAuthorization(Policies.Customer);
        MapProfile(me);
        MapAddresses(me.MapGroup("/addresses"));
        MapCart(me.MapGroup("/cart"));
        MapOrders(me.MapGroup("/orders"));
    }

    private static void MapProfile(RouteGroupBuilder me)
    {
        me.MapGet("/", async (ISender s, CancellationToken ct) => (await s.Send(new GetMeQuery(), ct)).ToOk())
            .WithName("GetMe").Produces<UserResponse>().ProducesAuthErrors();

        me.MapPut("/", async (UpdateProfileCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToNoContent())
            .WithName("UpdateMyProfile").Produces(StatusCodes.Status204NoContent).ProducesErrors().ProducesAuthErrors();

        me.MapPut("/password", async (ChangePasswordCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToNoContent())
            .WithName("ChangeMyPassword").WithSummary("Change password; signs out every session.")
            .Produces(StatusCodes.Status204NoContent).ProducesErrors().ProducesAuthErrors();
    }

    private static void MapAddresses(RouteGroupBuilder addresses)
    {
        addresses.MapGet("/", async (ISender s, CancellationToken ct) => (await s.Send(new ListMyAddressesQuery(), ct)).ToOk())
            .WithName("ListMyAddresses").Produces<IReadOnlyList<SavedAddressResponse>>().ProducesAuthErrors();

        addresses.MapPost("/", async (SaveAddressRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new AddMyAddressCommand(r.Label, r.Address, r.IsDefault), ct)).ToCreated(id => $"{ApiRoutes.MeV1}/addresses/{id}"))
            .WithName("AddMyAddress").Produces<Guid>(StatusCodes.Status201Created).ProducesErrors().ProducesAuthErrors();

        addresses.MapPut("/{id:guid}", async (Guid id, SaveAddressRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new UpdateMyAddressCommand(id, r.Label, r.Address), ct)).ToNoContent())
            .WithName("UpdateMyAddress").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        addresses.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) =>
                (await s.Send(new DeleteMyAddressCommand(id), ct)).ToNoContent())
            .WithName("DeleteMyAddress").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        addresses.MapPost("/{id:guid}/default", async (Guid id, ISender s, CancellationToken ct) =>
                (await s.Send(new SetDefaultAddressCommand(id), ct)).ToNoContent())
            .WithName("SetDefaultAddress").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();
    }

    private static void MapCart(RouteGroupBuilder cart)
    {
        cart.MapGet("/", async (ISender s, CancellationToken ct) => (await s.Send(new GetMyCartQuery(), ct)).ToOk())
            .WithName("GetMyCart").Produces<CartResponse>().ProducesAuthErrors();

        cart.MapPost("/items", async (AddCartItemCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToNoContent())
            .WithName("AddCartItem").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        cart.MapPut("/items/{variantId:guid}", async (Guid variantId, CartQuantityRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new UpdateCartItemCommand(variantId, r.Quantity), ct)).ToNoContent())
            .WithName("UpdateCartItem").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        cart.MapDelete("/items/{variantId:guid}", async (Guid variantId, ISender s, CancellationToken ct) =>
                (await s.Send(new RemoveCartItemCommand(variantId), ct)).ToNoContent())
            .WithName("RemoveCartItem").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        cart.MapPost("/merge", async (MergeCartCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToNoContent())
            .WithName("MergeCart").WithSummary("Merge the guest (browser) cart after sign-in.")
            .Produces(StatusCodes.Status204NoContent).ProducesErrors().ProducesAuthErrors();

        cart.MapDelete("/", async (ISender s, CancellationToken ct) => (await s.Send(new ClearCartCommand(), ct)).ToNoContent())
            .WithName("ClearCart").Produces(StatusCodes.Status204NoContent).ProducesAuthErrors();
    }

    private static void MapOrders(RouteGroupBuilder orders)
    {
        orders.MapGet("/", async ([AsParameters] ListMyOrdersQuery q, ISender s, CancellationToken ct) => (await s.Send(q, ct)).ToOk())
            .WithName("ListMyOrders").Produces<PagedResponse<OrderSummaryResponse>>().ProducesErrors().ProducesAuthErrors();

        orders.MapGet("/{orderNumber}", async (string orderNumber, ISender s, CancellationToken ct) =>
                (await s.Send(new GetMyOrderQuery(orderNumber), ct)).ToOk())
            .WithName("GetMyOrder").Produces<OrderDetailResponse>().ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        orders.MapPost("/{orderNumber}/cancel", async (string orderNumber, CancelRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new CancelMyOrderCommand(orderNumber, r.Reason), ct)).ToNoContent())
            .WithName("CancelMyOrder").WithSummary("Cancel my order while it is still pending.")
            .Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();
    }
}
