using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Reports.GetTopProducts;

/// <summary>Best sellers by quantity among delivered orders between two Vietnam dates (inclusive).</summary>
public sealed record GetTopProductsQuery(DateOnly From, DateOnly To, int Limit = 10) : IQuery<IReadOnlyList<TopProductResponse>>;
