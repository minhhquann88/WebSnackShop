using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.DeliverOrder;

/// <summary>Marks the order delivered: COD becomes paid and product sold counts increase.</summary>
public sealed record DeliverOrderCommand(Guid OrderId) : ICommand;
