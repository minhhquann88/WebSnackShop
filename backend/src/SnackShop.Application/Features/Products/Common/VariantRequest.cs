using FluentValidation;
using SnackShop.Application.Common;
using SnackShop.Application.Common.Validation;
using SnackShop.Domain.Catalog;
using SnackShop.Domain.Common;

namespace SnackShop.Application.Features.Products.Common;

/// <summary>Wire input for a variant. Empty <see cref="Name"/> means the default (unnamed) variant.</summary>
public sealed record VariantRequest(
    string? Name,
    string Sku,
    MoneyRequest Price,
    MoneyRequest? CompareAtPrice,
    int? WeightGrams,
    int SortOrder)
{
    public Result<VariantSpec> ToSpec()
    {
        var sku = Domain.Catalog.Sku.Create(Sku);
        if (sku.IsFailure)
        {
            return Result.Failure<VariantSpec>(sku.Error);
        }

        return new VariantSpec(Name ?? string.Empty, sku.Value, Price.ToMoney(), CompareAtPrice?.ToMoney(), WeightGrams, SortOrder);
    }
}

public sealed class VariantRequestValidator : AbstractValidator<VariantRequest>
{
    public VariantRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(ProductVariant.MaxNameLength);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(Domain.Catalog.Sku.MaxLength)
            .Matches("^[A-Za-z0-9-]{3,50}$").WithMessage("SKU must be 3-50 letters, digits or hyphens.");
        RuleFor(x => x.Price).NotNull().SetValidator(new MoneyRequestValidator());
        RuleFor(x => x.Price.Amount).GreaterThan(0).When(x => x.Price is not null);
        RuleFor(x => x.CompareAtPrice!).SetValidator(new MoneyRequestValidator()).When(x => x.CompareAtPrice is not null);
        RuleFor(x => x.WeightGrams).InclusiveBetween(1, 100_000).When(x => x.WeightGrams is not null);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
