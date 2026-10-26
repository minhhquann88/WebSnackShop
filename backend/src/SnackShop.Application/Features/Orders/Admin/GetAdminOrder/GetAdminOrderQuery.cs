using SnackShop.Application.Abstractions.Messaging;

namespace SnackShop.Application.Features.Orders.Admin.GetAdminOrder;

public sealed record GetAdminOrderQuery(Guid Id) : IQuery<AdminOrderDetailResponse>;
