using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.CancelOrder;

/// <summary>Staff cancels an order at any non-final stage; stock is released or returned per docs/03 §4.</summary>
public sealed record CancelOrderCommand(Guid OrderId, string Reason) : ICommand;
