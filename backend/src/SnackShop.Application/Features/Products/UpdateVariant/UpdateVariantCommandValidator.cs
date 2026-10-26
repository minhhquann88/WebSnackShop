using FluentValidation;
using SnackShop.Application.Features.Products.Common;

namespace SnackShop.Application.Features.Products.UpdateVariant;

public sealed class UpdateVariantCommandValidator : AbstractValidator<UpdateVariantCommand>
{
    public UpdateVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Variant).NotNull().SetValidator(new VariantRequestValidator());
    }
}
