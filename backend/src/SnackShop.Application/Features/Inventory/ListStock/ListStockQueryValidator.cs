using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Inventory.ListStock;

public sealed class ListStockQueryValidator : AbstractValidator<ListStockQuery>
{
    public ListStockQueryValidator()
    {
        this.ValidatePaging();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
