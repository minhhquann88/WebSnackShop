using FluentValidation;
using SnackShop.Domain.Inventory;

namespace SnackShop.Application.Features.Inventory.AdjustStock;

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Delta).NotEqual(0).InclusiveBetween(-1_000_000, 1_000_000);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(StockMovement.MaxNoteLength);
    }
}
