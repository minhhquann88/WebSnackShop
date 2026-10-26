using MediatR;
using SnackShop.Api.Contracts;
using SnackShop.Api.Extensions;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Customers;
using SnackShop.Application.Features.Customers.GetCustomer;
using SnackShop.Application.Features.Customers.ListCustomers;
using SnackShop.Application.Features.Users;
using SnackShop.Application.Features.Users.CreateStaff;
using SnackShop.Application.Features.Users.ListUsers;
using SnackShop.Application.Features.Users.SetUserLock;
using SnackShop.Application.Features.Users.SetUserRoles;

namespace SnackShop.Api.Endpoints.Admin;

/// <summary>Customers (Staff) and staff accounts (Admin).</summary>
public sealed class AdminPeopleEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var customers = app.MapGroup(ApiRoutes.AdminV1 + "/customers").WithTags("Admin · Customers").RequireAuthorization(Policies.Staff);

        customers.MapGet("/", async ([AsParameters] ListCustomersQuery q, ISender s, CancellationToken ct) => (await s.Send(q, ct)).ToOk())
            .WithName("AdminListCustomers").Produces<PagedResponse<CustomerSummaryResponse>>().ProducesErrors().ProducesAuthErrors();

        customers.MapGet("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new GetCustomerQuery(id), ct)).ToOk())
            .WithName("AdminGetCustomer").Produces<CustomerDetailResponse>().ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        var users = app.MapGroup(ApiRoutes.AdminV1 + "/users").WithTags("Admin · Users").RequireAuthorization(Policies.Admin);

        users.MapGet("/", async ([AsParameters] ListUsersQuery q, ISender s, CancellationToken ct) => (await s.Send(q, ct)).ToOk())
            .WithName("AdminListUsers").Produces<PagedResponse<AdminUserResponse>>().ProducesErrors().ProducesAuthErrors();

        users.MapPost("/", async (CreateStaffCommand c, ISender s, CancellationToken ct) =>
                (await s.Send(c, ct)).ToCreated(id => $"{ApiRoutes.AdminV1}/users/{id}"))
            .WithName("AdminCreateStaff").WithSummary("Create an employee account (Staff, optionally Admin).")
            .Produces<Guid>(StatusCodes.Status201Created).ProducesErrors(StatusCodes.Status409Conflict).ProducesAuthErrors();

        users.MapPut("/{id:guid}/roles", async (Guid id, SetRolesRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new SetUserRolesCommand(id, r.Roles), ct)).ToNoContent())
            .WithName("AdminSetUserRoles").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        users.MapPost("/{id:guid}/lock", async (Guid id, SetLockRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new SetUserLockCommand(id, r.Locked), ct)).ToNoContent())
            .WithName("AdminSetUserLock").WithSummary("Lock or unlock an account.")
            .Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();
    }
}
