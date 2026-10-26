using FluentValidation;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Application.Features.Products.AddVariant;

public sealed class AddVariantCommandValidator : AbstractValidator<AddVariantCommand>
{
    public AddVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Variant).NotNull().SetValidator(new VariantRequestValidator());
        RuleFor(x => x.InitialStock).InclusiveBetween(0, 1_000_000);
    }
}
