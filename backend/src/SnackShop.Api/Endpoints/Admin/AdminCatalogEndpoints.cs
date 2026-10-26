using MediatR;
using Microsoft.AspNetCore.Mvc;
using SnackShop.Api.Contracts;
using SnackShop.Api.Extensions;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Authorization;
using SnackShop.Application.Features.Categories;
using SnackShop.Application.Features.Categories.CreateCategory;
using SnackShop.Application.Features.Categories.DeleteCategory;
using SnackShop.Application.Features.Categories.ListAdminCategories;
using SnackShop.Application.Features.Categories.ReorderCategories;
using SnackShop.Application.Features.Categories.UpdateCategory;
using SnackShop.Application.Features.Products;
using SnackShop.Application.Features.Products.AddVariant;
using SnackShop.Application.Features.Products.CreateProduct;
using SnackShop.Application.Features.Products.DeleteProduct;
using SnackShop.Application.Features.Products.DeleteProductImage;
using SnackShop.Application.Features.Products.GetAdminProduct;
using SnackShop.Application.Features.Products.ListAdminProducts;
using SnackShop.Application.Features.Products.RemoveVariant;
using SnackShop.Application.Features.Products.ReorderImages;
using SnackShop.Application.Features.Products.SetPrimaryImage;
using SnackShop.Application.Features.Products.SetProductVisibility;
using SnackShop.Application.Features.Products.UpdateProduct;
using SnackShop.Application.Features.Products.UpdateVariant;
using SnackShop.Application.Features.Products.UploadProductImage;

namespace SnackShop.Api.Endpoints.Admin;

/// <summary>Catalog management (Admin only).</summary>
public sealed class AdminCatalogEndpoints : IEndpointGroup
{
    private const long MaxUploadBytes = 6 * 1024 * 1024;

    public void Map(IEndpointRouteBuilder app)
    {
        MapCategories(app.MapGroup(ApiRoutes.AdminV1 + "/categories").WithTags("Admin · Catalog").RequireAuthorization(Policies.Admin));
        var products = app.MapGroup(ApiRoutes.AdminV1 + "/products").WithTags("Admin · Catalog").RequireAuthorization(Policies.Admin);
        MapProducts(products);
        MapVariants(products);
        MapImages(products);
    }

    private static void MapCategories(RouteGroupBuilder g)
    {
        g.MapGet("/", async (ISender s, CancellationToken ct) => (await s.Send(new ListAdminCategoriesQuery(), ct)).ToOk())
            .WithName("AdminListCategories").Produces<IReadOnlyList<AdminCategoryResponse>>().ProducesAuthErrors();

        g.MapPost("/", async (CreateCategoryCommand c, ISender s, CancellationToken ct) =>
                (await s.Send(c, ct)).ToCreated(id => $"{ApiRoutes.AdminV1}/categories/{id}"))
            .WithName("AdminCreateCategory").Produces<Guid>(StatusCodes.Status201Created)
            .ProducesErrors(StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new UpdateCategoryCommand(id, r.Name, r.Slug, r.Description, r.ImageUrl, r.SortOrder, r.IsActive), ct)).ToNoContent())
            .WithName("AdminUpdateCategory").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeleteCategoryCommand(id), ct)).ToNoContent())
            .WithName("AdminDeleteCategory").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapPut("/order", async (ReorderRequest r, ISender s, CancellationToken ct) => (await s.Send(new ReorderCategoriesCommand(r.Ids), ct)).ToNoContent())
            .WithName("AdminReorderCategories").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();
    }

    private static void MapProducts(RouteGroupBuilder g)
    {
        g.MapGet("/", async ([AsParameters] ListAdminProductsQuery q, ISender s, CancellationToken ct) => (await s.Send(q, ct)).ToOk())
            .WithName("AdminListProducts").Produces<PagedResponse<AdminProductSummaryResponse>>().ProducesErrors().ProducesAuthErrors();

        g.MapGet("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new GetAdminProductQuery(id), ct)).ToOk())
            .WithName("AdminGetProduct").Produces<AdminProductDetailResponse>().ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        g.MapPost("/", async (CreateProductCommand c, ISender s, CancellationToken ct) =>
                (await s.Send(c, ct)).ToCreated(id => $"{ApiRoutes.AdminV1}/products/{id}"))
            .WithName("AdminCreateProduct").WithSummary("Create a product with its first variant and initial stock.")
            .Produces<Guid>(StatusCodes.Status201Created).ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new UpdateProductCommand(id, r.CategoryId, r.Name, r.Slug, r.ShortDescription, r.Description,
                    r.Ingredients, r.Origin, r.Brand, r.ShelfLifeDays), ct)).ToNoContent())
            .WithName("AdminUpdateProduct").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapPatch("/{id:guid}/status", async (Guid id, ProductVisibilityRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new SetProductVisibilityCommand(id, r.IsActive, r.IsFeatured), ct)).ToNoContent())
            .WithName("AdminSetProductVisibility").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapDelete("/{id:guid}", async (Guid id, ISender s, CancellationToken ct) => (await s.Send(new DeleteProductCommand(id), ct)).ToNoContent())
            .WithName("AdminDeleteProduct").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();
    }

    private static void MapVariants(RouteGroupBuilder g)
    {
        g.MapPost("/{id:guid}/variants", async (Guid id, AddVariantRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new AddVariantCommand(id, r.Variant, r.InitialStock), ct)).ToCreated(v => $"{ApiRoutes.AdminV1}/products/{id}"))
            .WithName("AdminAddVariant").Produces<Guid>(StatusCodes.Status201Created)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapPut("/{id:guid}/variants/{variantId:guid}", async (Guid id, Guid variantId, UpdateVariantRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new UpdateVariantCommand(id, variantId, r.Variant, r.IsActive), ct)).ToNoContent())
            .WithName("AdminUpdateVariant").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();

        g.MapDelete("/{id:guid}/variants/{variantId:guid}", async (Guid id, Guid variantId, ISender s, CancellationToken ct) =>
                (await s.Send(new RemoveVariantCommand(id, variantId), ct)).ToNoContent())
            .WithName("AdminRemoveVariant").Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict).ProducesAuthErrors();
    }

    private static void MapImages(RouteGroupBuilder g)
    {
        g.MapPost("/{id:guid}/images", UploadImageAsync)
            .WithName("AdminUploadProductImage").WithSummary("Upload a JPEG/PNG/WebP image (max 5 MB) as multipart/form-data.")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxUploadBytes))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ProductImageResponse>(StatusCodes.Status201Created).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        g.MapDelete("/{id:guid}/images/{imageId:guid}", async (Guid id, Guid imageId, ISender s, CancellationToken ct) =>
                (await s.Send(new DeleteProductImageCommand(id, imageId), ct)).ToNoContent())
            .WithName("AdminDeleteProductImage").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        g.MapPost("/{id:guid}/images/{imageId:guid}/primary", async (Guid id, Guid imageId, ISender s, CancellationToken ct) =>
                (await s.Send(new SetPrimaryImageCommand(id, imageId), ct)).ToNoContent())
            .WithName("AdminSetPrimaryImage").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();

        g.MapPut("/{id:guid}/images/order", async (Guid id, ReorderRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new ReorderImagesCommand(id, r.Ids), ct)).ToNoContent())
            .WithName("AdminReorderImages").Produces(StatusCodes.Status204NoContent).ProducesErrors(StatusCodes.Status404NotFound).ProducesAuthErrors();
    }

    private static async Task<IResult> UploadImageAsync(Guid id, IFormFile file, [FromForm] string? altText, ISender sender, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var result = await sender.Send(new UploadProductImageCommand(id, stream, file.Length, altText), ct);
        return result.ToCreated(_ => $"{ApiRoutes.AdminV1}/products/{id}");
    }
}
