using FluentValidation;

namespace SnackShop.Application.Features.Reports.GetTopProducts;

public sealed class GetTopProductsQueryValidator : AbstractValidator<GetTopProductsQuery>
{
    public GetTopProductsQueryValidator()
    {
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From);
        RuleFor(x => x.Limit).InclusiveBetween(1, 50);
    }
}
