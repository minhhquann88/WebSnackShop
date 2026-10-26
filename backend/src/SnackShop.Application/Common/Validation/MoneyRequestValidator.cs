using FluentValidation;

namespace SnackShop.Application.Common.Validation;

/// <summary>Shared rules for money inputs: non-negative, 3-letter currency, at most 2 decimals.</summary>
public sealed class MoneyRequestValidator : AbstractValidator<MoneyRequest>
{
    public MoneyRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
