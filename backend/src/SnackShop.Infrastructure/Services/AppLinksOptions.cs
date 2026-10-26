using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using SnackShop.Application.Abstractions.Services;

namespace SnackShop.Infrastructure.Services;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    [Required]
    [Url]
    public string BaseUrl { get; init; } = "http://localhost:3000";

    [Required]
    public string ShopName { get; init; } = "SnackShop";
}

public sealed class AppLinks(IOptions<FrontendOptions> options) : IAppLinks
{
    public string ShopName => options.Value.ShopName;

    public string Frontend(string relativePath) =>
        options.Value.BaseUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/');
}
