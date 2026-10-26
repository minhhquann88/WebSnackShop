using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.ExpireUnpaidOrders;

/// <summary>
/// System cancellation of one unpaid bank-transfer order past its deadline; releases the reserved stock.
/// Returns false (no-op) if the order was paid or changed meanwhile.
/// </summary>
public sealed record ExpireUnpaidOrderCommand(Guid OrderId) : ICommand<bool>;
