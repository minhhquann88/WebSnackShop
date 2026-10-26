using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.ExpireUnpaidOrders;

/// <summary>
/// Pending bank-transfer orders still unpaid after ShopSettings.UnpaidBankTransferExpiryHours, oldest first.
/// The maintenance job then expires each one with its own <see cref="ExpireUnpaidOrderCommand"/> (own transaction),
/// so one problematic order never blocks the others.
/// </summary>
public sealed record GetExpiredUnpaidOrderIdsQuery(int BatchSize = 50) : IQuery<IReadOnlyList<Guid>>;
