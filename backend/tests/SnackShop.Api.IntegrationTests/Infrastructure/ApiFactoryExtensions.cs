using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common.Authorization;

namespace SnackShop.Api.IntegrationTests.Infrastructure;

public static class ApiFactoryExtensions
{
    /// <summary>Client carrying a real signed JWT for a synthetic user with the given roles. No DB user is required.</summary>
    public static HttpClient CreateClientWithRoles(this ApiFactory factory, params string[] roles)
    {
        var jwt = factory.Services.GetRequiredService<IJwtProvider>();
        var userId = Guid.CreateVersion7();
        var token = jwt.GenerateAccessToken(userId, $"{userId:N}@test.local", "Test User", roles);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }

    public static HttpClient CreateAdminClient(this ApiFactory factory) => factory.CreateClientWithRoles(Roles.Admin, Roles.Staff);

    public static HttpClient CreateStaffClient(this ApiFactory factory) => factory.CreateClientWithRoles(Roles.Staff);

    public static HttpClient CreateCustomerClient(this ApiFactory factory) => factory.CreateClientWithRoles(Roles.Customer);
}
