using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Features.Orders.Admin.Common;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Orders.Admin.DeliverOrder;

public sealed class DeliverOrderCommandHandler(StaffOrderAccess access, IProductRepository products)
    : ICommandHandler<DeliverOrderCommand>
{
    public async Task<Result> Handle(DeliverOrderCommand command, CancellationToken cancellationToken)
    {
        var ctx = await access.LoadAsync(command.OrderId, cancellationToken);
        if (ctx.IsFailure)
        {
            return ctx;
        }

        var order = ctx.Value.Order;
        var delivered = order.MarkDelivered(ctx.Value.Actor, ctx.Value.Now);
        if (delivered.IsFailure)
        {
            return delivered;
        }

        var soldByProduct = order.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
        foreach (var product in await products.GetByIdsAsync(soldByProduct.Keys, cancellationToken))
        {
            product.RecordSale(soldByProduct[product.Id]);
        }

        return Result.Success();
    }
}
