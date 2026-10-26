using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Auth;
using SnackShop.Application.Features.Products;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Api.IntegrationTests.Infrastructure;

/// <summary>HTTP helpers that use the public API exactly like a client would.</summary>
public static class TestApi
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static AddressRequest Address(string phone = "0912345678") =>
        new("Nguyễn Văn Khách", phone, "12 Lê Lợi", "Bến Nghé", "Quận 1", "TP Hồ Chí Minh");

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
        var body = await response.Content.ReadFromJsonAsync<T>(Json, Ct);
        return body ?? throw new ShouldAssertException($"Response body was empty ({(int)response.StatusCode}).");
    }

    public static async Task<ProblemDetails> ProblemAsync(this HttpResponseMessage response) =>
        await response.ReadAsync<ProblemDetails>();

    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PostAsJsonAsync(url, body, Json, Ct);

    public static Task<HttpResponseMessage> PatchJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PatchAsJsonAsync(url, body, Json, Ct);

    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PutAsJsonAsync(url, body, Json, Ct);

    public static async Task<HttpResponseMessage> ShouldBeAsync(this Task<HttpResponseMessage> request, HttpStatusCode expected)
    {
        ArgumentNullException.ThrowIfNull(request);
        var response = await request;
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync(Ct);
            throw new ShouldAssertException($"Expected {(int)expected} {expected} but got {(int)response.StatusCode}: {body}");
        }

        return response;
    }

    public static HttpClient WithToken(this HttpClient client, string accessToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    /// <summary>Registers a real customer account and returns an authenticated client.</summary>
    public static async Task<(HttpClient Client, AuthResponse Auth)> RegisterCustomerAsync(this ApiFactory factory, string? email = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        var anonymous = factory.CreateClient();
        email ??= $"khach-{Guid.NewGuid():N}@example.com";

        var response = await anonymous.PostJsonAsync("/api/v1/auth/register",
            new { email, password = "Matkhau123", fullName = "Nguyễn Văn Khách", phoneNumber = "0912345678" })
            .ShouldBeAsync(HttpStatusCode.OK);
        var auth = await response.ReadAsync<AuthResponse>();
        return (factory.CreateClient().WithToken(auth.AccessToken), auth);
    }

    /// <summary>Signs in as the seeded admin (real account with Admin + Staff roles).</summary>
    public static async Task<HttpClient> LoginAdminAsync(this ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        var response = await factory.CreateClient()
            .PostJsonAsync("/api/v1/auth/login", new { email = ApiFactory.AdminEmail, password = ApiFactory.AdminPassword })
            .ShouldBeAsync(HttpStatusCode.OK);
        return factory.CreateClient().WithToken((await response.ReadAsync<AuthResponse>()).AccessToken);
    }

    /// <summary>Creates a category and an active product with one variant and the given stock. Returns the product detail.</summary>
    public static async Task<AdminProductDetailResponse> CreateProductAsync(
        this HttpClient admin,
        string name = "Khô gà lá chanh",
        decimal price = 50_000m,
        int stock = 10,
        string? sku = null)
    {
        ArgumentNullException.ThrowIfNull(admin);
        var categoryResponse = await admin.PostJsonAsync("/api/v1/admin/categories",
            new { name = $"Danh mục {Guid.NewGuid():N}"[..20], sortOrder = 0 }).ShouldBeAsync(HttpStatusCode.Created);
        var categoryId = await categoryResponse.ReadAsync<Guid>();

        var variant = new VariantRequest(null, sku ?? $"SKU-{Guid.NewGuid():N}"[..12].ToUpperInvariant(), new MoneyRequest(price), null, 200, 0);
        var productResponse = await admin.PostJsonAsync("/api/v1/admin/products", new
        {
            categoryId,
            name = $"{name} {Guid.NewGuid():N}"[..(name.Length + 9)],
            shortDescription = "Ngon",
            isActive = true,
            isFeatured = false,
            firstVariant = variant,
            initialStock = stock,
        }).ShouldBeAsync(HttpStatusCode.Created);
        var productId = await productResponse.ReadAsync<Guid>();

        var detail = await admin.GetAsync($"/api/v1/admin/products/{productId}", Ct).ShouldBeAsync(HttpStatusCode.OK);
        return await detail.ReadAsync<AdminProductDetailResponse>();
    }

    public static object OrderBody(Guid variantId, int quantity = 1, string payment = "Cod", string email = "khach@example.com", string phone = "0912345678") =>
        new
        {
            items = new[] { new { variantId, quantity } },
            contactEmail = email,
            shippingAddress = Address(phone),
            paymentMethod = payment,
            customerNote = "Giao giờ hành chính",
        };
}
