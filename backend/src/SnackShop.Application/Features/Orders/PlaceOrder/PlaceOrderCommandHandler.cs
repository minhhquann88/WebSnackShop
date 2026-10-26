using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Services;
using SnackShop.Application.Common;
using SnackShop.Application.Features.Me.Common;
using SnackShop.Application.Features.Orders.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Customers;
using SnackShop.Domain.Inventory;
using SnackShop.Domain.Ordering;
using SnackShop.Domain.Settings;

namespace SnackShop.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orders,
    IShopSettingsRepository settingsRepository,
    ICartRepository carts,
    ICurrentUser currentUser,
    CustomerProfiles profiles,
    OrderLineFactory lineFactory,
    OrderInventory inventory,
    BankTransferInstructions bankTransfer,
    TimeProvider timeProvider) : ICommandHandler<PlaceOrderCommand, PlaceOrderResponse>
{
    public async Task<Result<PlaceOrderResponse>> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.IdempotencyKey is { } key && await orders.GetByIdempotencyKeyAsync(key, cancellationToken) is { } existing)
        {
            // Only the same buyer may replay a key; otherwise a guessed key would reveal someone else's order.
            return string.Equals(existing.ContactEmail, command.ContactEmail.Trim(), StringComparison.OrdinalIgnoreCase)
                ? await ToResponseAsync(existing, cancellationToken)
                : Result.Failure<PlaceOrderResponse>(OrderErrors.IdempotencyKeyReused);
        }

        var settings = await settingsRepository.GetAsync(cancellationToken);
        var lines = await lineFactory.BuildAsync(command.Items, cancellationToken);
        if (settings is null || lines.IsFailure)
        {
            return Result.Failure<PlaceOrderResponse>(settings is null ? SettingsErrors.NotSeeded : lines.Error);
        }

        Guid? customerId = null;
        if (currentUser.IsAuthenticated)
        {
            var customer = await profiles.GetOrCreateAsync(cancellationToken);
            customerId = customer.IsSuccess ? customer.Value.Id : null;
        }

        var now = timeProvider.GetUtcNow();
        var subtotal = lines.Value.Aggregate(Money.Zero(), (sum, l) => sum + (l.UnitPrice * l.Quantity));
        var data = new PlaceOrderData(await orders.NextOrderNumberAsync(now, cancellationToken), customerId, command.ContactEmail,
            command.ShippingAddress.ToInfo(), command.PaymentMethod, command.CustomerNote, command.IdempotencyKey);

        var placed = Order.Place(data, lines.Value, settings.CalculateShippingFee(subtotal), now);
        if (placed.IsFailure)
        {
            return Result.Failure<PlaceOrderResponse>(placed.Error);
        }

        var reserved = await inventory.ReserveAsync(placed.Value, new StockContext(now, currentUser.UserId), cancellationToken);
        if (reserved.IsFailure)
        {
            return Result.Failure<PlaceOrderResponse>(reserved.Error);
        }

        orders.Add(placed.Value);
        if (customerId is { } id && await carts.GetByCustomerIdAsync(id, cancellationToken) is { } cart)
        {
            cart.RemoveVariants(lines.Value.Select(l => l.VariantId).ToList(), now);
        }

        return await ToResponseAsync(placed.Value, cancellationToken);
    }

    private async Task<Result<PlaceOrderResponse>> ToResponseAsync(Order order, CancellationToken cancellationToken) =>
        new PlaceOrderResponse(order.Id, order.OrderNumber.Value, MoneyResponse.From(order.Total), order.PaymentMethod,
            await bankTransfer.ForOrderAsync(order, cancellationToken));
}
