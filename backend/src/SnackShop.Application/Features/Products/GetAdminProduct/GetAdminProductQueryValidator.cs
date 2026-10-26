using FluentValidation;

namespace SnackShop.Application.Features.Products.GetAdminProduct;

public sealed class GetAdminProductQueryValidator : AbstractValidator<GetAdminProductQuery>
{
    public GetAdminProductQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
