using FluentValidation;

namespace SnackShop.Application.Features.Inventory.SetLowStockThreshold;

public sealed class SetLowStockThresholdCommandValidator : AbstractValidator<SetLowStockThresholdCommand>
{
    public SetLowStockThresholdCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Threshold).InclusiveBetween(0, 100_000);
    }
}
