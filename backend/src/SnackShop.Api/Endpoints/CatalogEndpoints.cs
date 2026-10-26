using MediatR;
using SnackShop.Api.Extensions;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Categories;
using SnackShop.Application.Features.Categories.ListCategories;
using SnackShop.Application.Features.Products;
using SnackShop.Application.Features.Products.GetProductBySlug;
using SnackShop.Application.Features.Products.ListProducts;

namespace SnackShop.Api.Endpoints;

/// <summary>Public storefront catalog.</summary>
public sealed class CatalogEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        var categories = app.MapGroup(ApiRoutes.V1 + "/categories").WithTags("Catalog").AllowAnonymous();
        categories.MapGet("/", ListCategoriesAsync)
            .WithName("ListCategories")
            .WithSummary("Active categories for the storefront menu.")
            .Produces<IReadOnlyList<CategoryResponse>>();

        var products = app.MapGroup(ApiRoutes.V1 + "/products").WithTags("Catalog").AllowAnonymous();
        products.MapGet("/", ListProductsAsync)
            .WithName("ListProducts")
            .WithSummary("Product listing with category filter, accent-insensitive search, sort and paging.")
            .Produces<PagedResponse<ProductSummaryResponse>>()
            .ProducesErrors();

        products.MapGet("/{slug}", GetProductAsync)
            .WithName("GetProductBySlug")
            .WithSummary("Product page: details, images, variants with available stock.")
            .Produces<ProductDetailResponse>()
            .ProducesErrors(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ListCategoriesAsync(ISender sender, CancellationToken ct) =>
        (await sender.Send(new ListCategoriesQuery(), ct)).ToOk();

    private static async Task<IResult> ListProductsAsync([AsParameters] ListProductsQuery query, ISender sender, CancellationToken ct) =>
        (await sender.Send(query, ct)).ToOk();

    private static async Task<IResult> GetProductAsync(string slug, ISender sender, CancellationToken ct) =>
        (await sender.Send(new GetProductBySlugQuery(slug), ct)).ToOk();
}
