using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.GetMyOrder;

public sealed record GetMyOrderQuery(string OrderNumber) : IQuery<OrderDetailResponse>;
