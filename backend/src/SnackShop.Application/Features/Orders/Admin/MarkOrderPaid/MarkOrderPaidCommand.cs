using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.MarkOrderPaid;

/// <summary>Records that payment was received (bank transfer reconciliation). <see cref="PaidAt"/> defaults to now.</summary>
public sealed record MarkOrderPaidCommand(Guid OrderId, string? TransactionRef, DateTimeOffset? PaidAt) : ICommand;
