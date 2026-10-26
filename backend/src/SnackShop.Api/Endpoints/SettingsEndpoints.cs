using MediatR;
using SnackShop.Api.Extensions;
using SnackShop.Application.Features.Settings.GetPublicSettings;

namespace SnackShop.Api.Endpoints;

public sealed class SettingsEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.V1 + "/settings").WithTags("Settings");

        group.MapGet("/public", GetPublicSettingsAsync)
            .WithName("GetPublicSettings")
            .WithSummary("Shop contact, shipping policy and bank account shown on the storefront.")
            .AllowAnonymous()
            .Produces<PublicSettingsResponse>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetPublicSettingsAsync(ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetPublicSettingsQuery(), ct)).ToOk();
}
