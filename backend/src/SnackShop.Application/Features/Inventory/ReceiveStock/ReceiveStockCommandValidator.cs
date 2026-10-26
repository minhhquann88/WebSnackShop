using FluentValidation;
using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Inventory.ReceiveStock;

public sealed class ReceiveStockCommandValidator : AbstractValidator<ReceiveStockCommand>
{
    public ReceiveStockCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, 1_000_000);
        RuleFor(x => x.Note).MaximumLength(StockMovement.MaxNoteLength);
    }
}
