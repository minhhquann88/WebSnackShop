using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Inventory.ListStockMovements;

public sealed class ListStockMovementsQueryValidator : AbstractValidator<ListStockMovementsQuery>
{
    public ListStockMovementsQueryValidator()
    {
        this.ValidatePaging();
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
