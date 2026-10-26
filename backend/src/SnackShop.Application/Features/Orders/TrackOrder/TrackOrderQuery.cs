using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.TrackOrder;

/// <summary>Guest order tracking: the order number plus the delivery phone number act as the credential.</summary>
public sealed record TrackOrderQuery(string OrderNumber, string Phone) : IQuery<OrderDetailResponse>;
