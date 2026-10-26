using FluentValidation;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Application.Features.Products.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        this.AddProductFieldRules(
            x => x.Name, x => x.Slug, x => x.ShortDescription, x => x.Description,
            x => x.Ingredients, x => x.Origin, x => x.Brand, x => x.ShelfLifeDays);
        RuleFor(x => x.FirstVariant).NotNull().SetValidator(new VariantRequestValidator());
        RuleFor(x => x.InitialStock).InclusiveBetween(0, 1_000_000);
    }
}
