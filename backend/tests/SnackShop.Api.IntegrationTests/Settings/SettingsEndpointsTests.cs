using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using SnackShop.Api.IntegrationTests.Infrastructure;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Settings.GetAdminSettings;
using SnackShop.Application.Features.Settings.GetPublicSettings;
using SnackShop.Application.Features.Settings.UpdateSettings;

namespace SnackShop.Api.IntegrationTests.Settings;

[Collection(ApiCollection.Name)]
public sealed class SettingsEndpointsTests(ApiFactory factory) : IAsyncLifetime
{
    private const string PublicUrl = "/api/v1/settings/public";
    private const string AdminUrl = "/api/v1/admin/settings";

    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static UpdateSettingsCommand ValidUpdate() => new(
        ShopName: "Snack House",
        ShopPhone: "0912345678",
        ShopAddress: "123 Lê Lợi, Quận 1",
        FlatShippingFee: new MoneyRequest(20_000m),
        FreeShippingThreshold: new MoneyRequest(250_000m),
        BankName: "Vietcombank",
        BankAccountNumber: "0011002233",
        BankAccountName: "NGUYEN VAN A",
        BankBin: "970436",
        UnpaidBankTransferExpiryHours: 12);

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPublic_AsAnonymous_ReturnsSeededSettings()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(PublicUrl, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PublicSettingsResponse>(TestContext.Current.CancellationToken);
        body.ShouldNotBeNull();
        body.ShopName.ShouldBe("SnackShop");
        body.FlatShippingFee.Currency.ShouldBe("VND");
    }

    [Fact]
    public async Task GetAdmin_WithoutToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(AdminUrl, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsCustomer_Returns403()
    {
        var client = factory.CreateCustomerClient();

        var response = await client.PutAsJsonAsync(AdminUrl, ValidUpdate(), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_AsStaff_Returns403()
    {
        var client = factory.CreateStaffClient();

        var response = await client.PutAsJsonAsync(AdminUrl, ValidUpdate(), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_AsAdmin_PersistsAndIsVisiblePublicly()
    {
        var admin = factory.CreateAdminClient();
        var anonymous = factory.CreateClient();

        var put = await admin.PutAsJsonAsync(AdminUrl, ValidUpdate(), TestContext.Current.CancellationToken);
        put.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var adminView = await admin.GetFromJsonAsync<AdminSettingsResponse>(AdminUrl, TestContext.Current.CancellationToken);
        adminView.ShouldNotBeNull();
        adminView.ShopName.ShouldBe("Snack House");
        adminView.UnpaidBankTransferExpiryHours.ShouldBe(12);
        adminView.UpdatedAt.ShouldNotBeNull();

        var publicView = await anonymous.GetFromJsonAsync<PublicSettingsResponse>(PublicUrl, TestContext.Current.CancellationToken);
        publicView.ShouldNotBeNull();
        publicView.FlatShippingFee.Amount.ShouldBe(20_000m);
        publicView.FreeShippingThreshold.ShouldNotBeNull().Amount.ShouldBe(250_000m);
    }

    [Fact]
    public async Task Update_WithInvalidInput_Returns400WithFieldErrors()
    {
        var admin = factory.CreateAdminClient();
        var command = ValidUpdate() with { ShopPhone = "abc", BankBin = "12" };

        var response = await admin.PutAsJsonAsync(AdminUrl, command, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Validation.Failed");
        problem.Errors.Keys.ShouldContain("shopPhone");
        problem.Errors.Keys.ShouldContain("bankBin");
    }

    [Fact]
    public async Task Update_WhenDomainRuleFails_Returns400WithErrorCode()
    {
        var admin = factory.CreateAdminClient();
        var command = ValidUpdate() with { FreeShippingThreshold = new MoneyRequest(5_000m) };

        var response = await admin.PutAsJsonAsync(AdminUrl, command, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Settings.FreeShippingThresholdTooLow");
        problem.Extensions.ShouldContainKey("traceId");
    }
}
