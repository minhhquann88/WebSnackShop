using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.CancelMyOrder;

/// <summary>The customer cancels their own pending order; the reserved stock is released.</summary>
public sealed record CancelMyOrderCommand(string OrderNumber, string? Reason) : ICommand;
