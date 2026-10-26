using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.ConfirmOrder;

/// <summary>
/// Accepts a pending order and commits its reserved stock. For bank transfers set <see cref="MarkPaid"/> when the
/// money has arrived (optionally with the bank <see cref="TransactionRef"/>).
/// </summary>
public sealed record ConfirmOrderCommand(Guid OrderId, bool MarkPaid = false, string? TransactionRef = null) : ICommand;
