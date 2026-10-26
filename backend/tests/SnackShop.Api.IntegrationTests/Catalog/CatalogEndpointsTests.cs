using System.Net;
using System.Net.Http.Headers;
using Shouldly;
using SnackShop.Api.IntegrationTests.Infrastructure;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Categories;
using SnackShop.Application.Features.Products;

namespace SnackShop.Api.IntegrationTests.Catalog;

[Collection(ApiCollection.Name)]
public sealed class CatalogEndpointsTests(ApiFactory factory) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task AdminCreatesProduct_ItAppearsInPublicListingAndDetail()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync("Khô bò miếng", 95_000m, stock: 7);
        var anonymous = factory.CreateClient();

        var list = await (await anonymous.GetAsync("/api/v1/products", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<PagedResponse<ProductSummaryResponse>>();
        var card = list.Items.ShouldHaveSingleItem();
        card.Price!.Amount.ShouldBe(95_000m);
        card.InStock.ShouldBeTrue();

        var detail = await (await anonymous.GetAsync($"/api/v1/products/{product.Slug}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<ProductDetailResponse>();
        detail.Variants.ShouldHaveSingleItem().AvailableQuantity.ShouldBe(7);

        var categories = await (await anonymous.GetAsync("/api/v1/categories", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<List<CategoryResponse>>();
        categories.ShouldHaveSingleItem().ProductCount.ShouldBe(1);
    }

    [Fact]
    public async Task Search_IsAccentAndCaseInsensitive()
    {
        var admin = await factory.LoginAdminAsync();
        await admin.CreateProductAsync("Khô gà lá chanh");
        await admin.CreateProductAsync("Bánh tráng trộn");

        var result = await (await factory.CreateClient().GetAsync("/api/v1/products?search=KHO%20GA", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<PagedResponse<ProductSummaryResponse>>();

        result.Items.ShouldHaveSingleItem().Name.ShouldStartWith("Khô gà lá chanh");
    }

    [Fact]
    public async Task SortByPrice_AndPaging_Work()
    {
        var admin = await factory.LoginAdminAsync();
        await admin.CreateProductAsync("A", 30_000m);
        await admin.CreateProductAsync("B", 10_000m);
        await admin.CreateProductAsync("C", 20_000m);

        var page = await (await factory.CreateClient().GetAsync("/api/v1/products?sort=price&pageSize=2", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<PagedResponse<ProductSummaryResponse>>();

        page.Items.Select(i => i.Price!.Amount).ShouldBe([10_000m, 20_000m]);
        page.TotalCount.ShouldBe(3);
        page.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task InvalidSort_Returns400()
    {
        await factory.CreateClient().GetAsync("/api/v1/products?sort=hacker", TestApi.Ct).ShouldBeAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HiddenProduct_IsNotPublic_ButVisibleToAdmin()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync();

        await admin.PatchJsonAsync($"/api/v1/admin/products/{product.Id}/status", new { isActive = false, isFeatured = false })
            .ShouldBeAsync(HttpStatusCode.NoContent);

        await factory.CreateClient().GetAsync($"/api/v1/products/{product.Slug}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.NotFound);
        var adminList = await (await admin.GetAsync("/api/v1/admin/products?isActive=false", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK))
            .ReadAsync<PagedResponse<AdminProductSummaryResponse>>();
        adminList.Items.ShouldHaveSingleItem().Id.ShouldBe(product.Id);
    }

    [Fact]
    public async Task DuplicateSku_Returns409()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync(sku: "DUP-SKU-1");

        var response = await admin.PostJsonAsync($"/api/v1/admin/products/{product.Id}/variants", new
        {
            variant = new { name = "500g", sku = "dup-sku-1", price = new { amount = 100_000m } },
        }).ShouldBeAsync(HttpStatusCode.Conflict);

        (await response.ProblemAsync()).Title.ShouldBe("Catalog.DuplicateSku");
    }

    [Fact]
    public async Task AddVariant_CreatesStockRecord()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync();

        await admin.PostJsonAsync($"/api/v1/admin/products/{product.Id}/variants", new
        {
            variant = new { name = "Hũ 500g", sku = "NEW-VAR-1", price = new { amount = 150_000m }, sortOrder = 1 },
            initialStock = 4,
        }).ShouldBeAsync(HttpStatusCode.Created);

        var detail = await (await admin.GetAsync($"/api/v1/admin/products/{product.Id}", TestApi.Ct)).ReadAsync<AdminProductDetailResponse>();
        detail.Variants.Single(v => v.Sku == "NEW-VAR-1").QuantityOnHand.ShouldBe(4);
    }

    [Fact]
    public async Task DeleteCategory_WithProducts_Returns409()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync();
        var detail = await (await admin.GetAsync($"/api/v1/admin/products/{product.Id}", TestApi.Ct)).ReadAsync<AdminProductDetailResponse>();

        var response = await admin.DeleteAsync($"/api/v1/admin/categories/{detail.CategoryId}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Conflict);

        (await response.ProblemAsync()).Title.ShouldBe("Catalog.CategoryHasProducts");
    }

    [Fact]
    public async Task DeleteProduct_IsSoft_AndSlugStaysReserved()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync();

        await admin.DeleteAsync($"/api/v1/admin/products/{product.Id}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.NoContent);

        await admin.GetAsync($"/api/v1/admin/products/{product.Id}", TestApi.Ct).ShouldBeAsync(HttpStatusCode.NotFound);
        var stillInDb = await factory.WithDbAsync(db => Task.FromResult(
            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.IgnoreQueryFilters(db.ProductSet).Any(p => p.Id == product.Id && p.IsDeleted)));
        stillInDb.ShouldBeTrue();
    }

    [Fact]
    public async Task UploadImage_AcceptsPng_RejectsFakeImage()
    {
        var admin = await factory.LoginAdminAsync();
        var product = await admin.CreateProductAsync();

        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4, 5, 6, 7, 8];
        using var good = new MultipartFormDataContent { { new ByteArrayContent(png) { Headers = { ContentType = new MediaTypeHeaderValue("image/png") } }, "file", "a.png" } };
        var uploaded = await (await admin.PostAsync($"/api/v1/admin/products/{product.Id}/images", good, TestApi.Ct)
            .ShouldBeAsync(HttpStatusCode.Created)).ReadAsync<ProductImageResponse>();
        uploaded.IsPrimary.ShouldBeTrue();
        uploaded.Url.ShouldEndWith(".png");

        using var fake = new MultipartFormDataContent { { new ByteArrayContent("hello"u8.ToArray()) { Headers = { ContentType = new MediaTypeHeaderValue("image/png") } }, "file", "x.png" } };
        var rejected = await admin.PostAsync($"/api/v1/admin/products/{product.Id}/images", fake, TestApi.Ct).ShouldBeAsync(HttpStatusCode.BadRequest);
        (await rejected.ProblemAsync()).Title.ShouldBe("Catalog.UnsupportedImage");
    }

    [Fact]
    public async Task CatalogAdmin_IsForbiddenForCustomersAndStaff()
    {
        var (customer, _) = await factory.RegisterCustomerAsync();
        await customer.GetAsync("/api/v1/admin/products", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Forbidden);
        await factory.CreateStaffClient().PostJsonAsync("/api/v1/admin/categories", new { name = "X" }).ShouldBeAsync(HttpStatusCode.Forbidden);
        await factory.CreateClient().GetAsync("/api/v1/admin/products", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Unauthorized);
    }
}
