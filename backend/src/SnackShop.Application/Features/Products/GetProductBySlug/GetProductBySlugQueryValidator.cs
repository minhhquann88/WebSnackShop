using FluentValidation;

namespace SnackShop.Application.Features.Products.GetProductBySlug;

public sealed class GetProductBySlugQueryValidator : AbstractValidator<GetProductBySlugQuery>
{
    public GetProductBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120);
    }
}
