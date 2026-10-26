using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Products.ListProducts;

public sealed class ListProductsQueryValidator : AbstractValidator<ListProductsQuery>
{
    public ListProductsQueryValidator()
    {
        this.ValidatePaging();
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.CategorySlug).MaximumLength(120);
        RuleFor(x => x.Sort)
            .Must(s => s is null || ListProductsQuery.AllowedSorts.Contains(s))
            .WithMessage($"Sort must be one of: {string.Join(", ", ListProductsQuery.AllowedSorts)}.");
    }
}
