using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Products.ListAdminProducts;

public sealed class ListAdminProductsQueryValidator : AbstractValidator<ListAdminProductsQuery>
{
    public ListAdminProductsQueryValidator()
    {
        this.ValidatePaging();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
