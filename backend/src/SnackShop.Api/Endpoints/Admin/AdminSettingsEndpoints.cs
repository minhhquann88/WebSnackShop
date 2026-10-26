using MediatR;
using SnackShop.Api.Extensions;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Settings.GetAdminSettings;
using SnackShop.Application.Features.Settings.UpdateSettings;

namespace SnackShop.Api.Endpoints.Admin;

public sealed class AdminSettingsEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.AdminV1 + "/settings")
            .WithTags("Admin · Settings")
            .RequireAuthorization(Policies.Admin);

        group.MapGet("/", GetSettingsAsync)
            .WithName("GetAdminSettings")
            .WithSummary("Full shop settings for the admin settings page.")
            .Produces<AdminSettingsResponse>()
            .ProducesAuthErrors();

        group.MapPut("/", UpdateSettingsAsync)
            .WithName("UpdateSettings")
            .WithSummary("Replace all shop settings.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesErrors()
            .ProducesAuthErrors();
    }

    private static async Task<IResult> GetSettingsAsync(ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetAdminSettingsQuery(), ct)).ToOk();

    private static async Task<IResult> UpdateSettingsAsync(UpdateSettingsCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToNoContent();
}
