using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Cart;

namespace SnackShop.Application.Features.Orders.EstimateOrder;

/// <summary>Prices a prospective order (checkout page): live prices, stock, shipping fee and total.</summary>
public sealed record EstimateOrderQuery(IReadOnlyList<CartLineRequest> Items) : IQuery<OrderEstimateResponse>;

public sealed record OrderEstimateResponse(
    IReadOnlyList<PricedLineResponse> Items,
    MoneyResponse Subtotal,
    MoneyResponse ShippingFee,
    MoneyResponse Total,
    MoneyResponse? FreeShippingThreshold,
    bool CanPlaceOrder,
    IReadOnlyList<Guid> UnavailableVariantIds);
