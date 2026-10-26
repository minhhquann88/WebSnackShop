using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Cart;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.PlaceOrder;

/// <summary>
/// Places an order for a guest or the signed-in customer. Prices come from the catalog, never from the client.
/// Retrying with the same <see cref="IdempotencyKey"/> returns the original order instead of creating a new one.
/// </summary>
public sealed record PlaceOrderCommand(
    IReadOnlyList<CartLineRequest> Items,
    string ContactEmail,
    AddressRequest ShippingAddress,
    PaymentMethod PaymentMethod,
    string? CustomerNote,
    string? IdempotencyKey = null) : ICommand<PlaceOrderResponse>;

public sealed record PlaceOrderResponse(
    Guid OrderId,
    string OrderNumber,
    MoneyResponse Total,
    PaymentMethod PaymentMethod,
    BankTransferInstructionsResponse? BankTransfer);
