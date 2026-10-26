using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.StartPreparingOrder;

public sealed record StartPreparingOrderCommand(Guid OrderId) : ICommand;
