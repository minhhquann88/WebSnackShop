using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Products.GetAdminProduct;

public sealed record GetAdminProductQuery(Guid Id) : IQuery<AdminProductDetailResponse>;
