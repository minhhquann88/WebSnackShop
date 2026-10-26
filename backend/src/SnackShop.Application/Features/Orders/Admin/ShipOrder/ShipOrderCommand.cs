using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.ShipOrder;

/// <summary>Hands the order to the shipper. <see cref="Note"/> can hold the carrier and tracking code.</summary>
public sealed record ShipOrderCommand(Guid OrderId, string? Note) : ICommand;
