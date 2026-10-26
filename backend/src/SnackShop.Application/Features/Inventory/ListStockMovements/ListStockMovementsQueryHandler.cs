using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Inventory.ListStockMovements;

public sealed class ListStockMovementsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListStockMovementsQuery, PagedResponse<StockMovementResponse>>
{
    public async Task<Result<PagedResponse<StockMovementResponse>>> Handle(ListStockMovementsQuery query, CancellationToken cancellationToken)
    {
        if (!await db.StockItems.AnyAsync(s => s.Id == query.VariantId, cancellationToken))
        {
            return Result.Failure<PagedResponse<StockMovementResponse>>(InventoryErrors.StockItemNotFound);
        }

        var page = await db.StockMovements.AsNoTracking()
            .Where(m => m.VariantId == query.VariantId)
            .OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id)
            .Select(m => new StockMovementResponse(
                m.Id, m.Type, m.Quantity, m.OnHandAfter, m.ReservedAfter, m.ReferenceType, m.ReferenceId, m.Note, m.CreatedAt, m.CreatedBy))
            .ToPagedResponseAsync(query.Page, query.PageSize, cancellationToken);

        return page;
    }
}
