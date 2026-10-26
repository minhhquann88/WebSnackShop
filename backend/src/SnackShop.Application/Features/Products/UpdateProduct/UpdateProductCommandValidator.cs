using FluentValidation;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Application.Features.Products.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        this.AddProductFieldRules(
            x => x.Name, x => x.Slug, x => x.ShortDescription, x => x.Description,
            x => x.Ingredients, x => x.Origin, x => x.Brand, x => x.ShelfLifeDays);
    }
}
