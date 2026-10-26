using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.MarkOrderRefunded;

/// <summary>Confirms the money of a cancelled, paid order was returned (payment RefundPending → Refunded).</summary>
public sealed record MarkOrderRefundedCommand(Guid OrderId, string? TransactionRef) : ICommand;
