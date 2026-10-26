using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.GetProductBySlug;

/// <summary>Product page for the storefront. Hidden products and hidden categories return not found.</summary>
public sealed record GetProductBySlugQuery(string Slug) : IQuery<ProductDetailResponse>;
