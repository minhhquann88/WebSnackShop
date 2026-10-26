using System.Linq.Expressions;
using FluentValidation;

namespace SnackShop.Application.Features.Cart.Common;

public sealed class CartLineRequestValidator : AbstractValidator<CartLineRequest>
{
    public CartLineRequestValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, Domain.Customers.Cart.MaxQuantityPerItem);
    }
}

/// <summary>Rules for a list of lines: bounded, each valid, no duplicate variants.</summary>
public static class CartLinesValidation
{
    public static void AddLinesRules<T>(this AbstractValidator<T> validator, Expression<Func<T, IReadOnlyList<CartLineRequest>>> lines)
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.RuleFor(lines).NotEmpty()
            .Must(l => l.Count <= Domain.Customers.Cart.MaxItems).WithMessage($"At most {Domain.Customers.Cart.MaxItems} different items.")
            .Must(l => l.Select(x => x.VariantId).Distinct().Count() == l.Count).WithMessage("Each variant may appear only once.")
            .ForEach(line => line.NotNull().SetValidator(new CartLineRequestValidator()));
    }
}
