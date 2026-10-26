using System.Net;
using Shouldly;
using SnackShop.Api.IntegrationTests.Infrastructure;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Auth;
using SnackShop.Application.Features.Customers;
using SnackShop.Application.Features.Users;

namespace SnackShop.Api.IntegrationTests.Admin;

[Collection(ApiCollection.Name)]
public sealed class AdminPeopleTests(ApiFactory factory) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task CreatedStaff_CanProcessOrders_ButNotManageCatalog()
    {
        var admin = await factory.LoginAdminAsync();
        await admin.PostJsonAsync("/api/v1/admin/users", new { email = "nhanvien@shop.vn", fullName = "Nhân viên", password = "Nhanvien123", isAdmin = false })
            .ShouldBeAsync(HttpStatusCode.Created);

        var login = await (await factory.CreateClient().PostJsonAsync("/api/v1/auth/login", new { email = "nhanvien@shop.vn", password = "Nhanvien123" })
            .ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<AuthResponse>();
        login.User.Roles.ShouldBe(["Staff"]);
        var staff = factory.CreateClient().WithToken(login.AccessToken);

        await staff.GetAsync("/api/v1/admin/orders", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK);
        await staff.GetAsync("/api/v1/admin/reports/dashboard", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK);
        await staff.GetAsync("/api/v1/admin/inventory", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK);
        await staff.GetAsync("/api/v1/admin/products", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Forbidden);
        await staff.GetAsync("/api/v1/admin/users", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LockedUser_CannotSignIn_AndLosesSessions()
    {
        var admin = await factory.LoginAdminAsync();
        var (_, auth) = await factory.RegisterCustomerAsync("bad@example.com");

        await admin.PostJsonAsync($"/api/v1/admin/users/{auth.User.Id}/lock", new { locked = true }).ShouldBeAsync(HttpStatusCode.NoContent);

        var login = await factory.CreateClient().PostJsonAsync("/api/v1/auth/login", new { email = "bad@example.com", password = "Matkhau123" })
            .ShouldBeAsync(HttpStatusCode.Unauthorized);
        (await login.ProblemAsync()).Title.ShouldBe("Auth.LockedOut");
        await factory.CreateClient().PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken }).ShouldBeAsync(HttpStatusCode.Unauthorized);

        await admin.PostJsonAsync($"/api/v1/admin/users/{auth.User.Id}/lock", new { locked = false }).ShouldBeAsync(HttpStatusCode.NoContent);
        await factory.CreateClient().PostJsonAsync("/api/v1/auth/login", new { email = "bad@example.com", password = "Matkhau123" }).ShouldBeAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Admin_CannotLockOrDemoteThemselves()
    {
        var admin = await factory.LoginAdminAsync();
        var users = await (await admin.GetAsync("/api/v1/admin/users?role=Admin", TestApi.Ct)).ReadAsync<PagedResponse<AdminUserResponse>>();
        var self = users.Items.Single(u => u.Email == ApiFactory.AdminEmail);

        await admin.PostJsonAsync($"/api/v1/admin/users/{self.Id}/lock", new { locked = true }).ShouldBeAsync(HttpStatusCode.Conflict);
        await admin.PutJsonAsync($"/api/v1/admin/users/{self.Id}/roles", new { roles = new[] { "Staff" } }).ShouldBeAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Customers_AreListedWithProfileAndAddresses()
    {
        var admin = await factory.LoginAdminAsync();
        var (customer, auth) = await factory.RegisterCustomerAsync("listed@example.com");
        await customer.PostJsonAsync("/api/v1/me/addresses", new { label = "Nhà", address = TestApi.Address() }).ShouldBeAsync(HttpStatusCode.Created);

        var list = await (await admin.GetAsync("/api/v1/admin/customers?search=listed", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<PagedResponse<CustomerSummaryResponse>>();
        list.Items.ShouldHaveSingleItem().Email.ShouldBe("listed@example.com");

        var detail = await (await admin.GetAsync($"/api/v1/admin/customers/{auth.User.Id}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<CustomerDetailResponse>();
        detail.Addresses.ShouldHaveSingleItem().IsDefault.ShouldBeTrue();
    }

    [Fact]
    public async Task AddressBook_CrudAndDefault()
    {
        var (customer, _) = await factory.RegisterCustomerAsync();

        var first = await (await customer.PostJsonAsync("/api/v1/me/addresses", new { label = "Nhà", address = TestApi.Address() })
            .ShouldBeAsync(HttpStatusCode.Created)).ReadAsync<Guid>();
        var second = await (await customer.PostJsonAsync("/api/v1/me/addresses", new { label = "Công ty", address = TestApi.Address("0987654321") }))
            .ReadAsync<Guid>();

        await customer.PostAsync($"/api/v1/me/addresses/{second}/default", null, TestApi.Ct).ShouldBeAsync(HttpStatusCode.NoContent);
        await customer.DeleteAsync($"/api/v1/me/addresses/{first}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.NoContent);

        var list = await (await customer.GetAsync("/api/v1/me/addresses", TestApi.Ct)).ReadAsync<List<Application.Features.Me.Addresses.SavedAddressResponse>>();
        list.ShouldHaveSingleItem().IsDefault.ShouldBeTrue();
        await customer.PutJsonAsync($"/api/v1/me/addresses/{first}", new { label = "x", address = TestApi.Address() }).ShouldBeAsync(HttpStatusCode.NotFound);
    }
}
