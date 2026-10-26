using FluentValidation;

namespace SnackShop.Application.Features.Products.RemoveVariant;

public sealed class RemoveVariantCommandValidator : AbstractValidator<RemoveVariantCommand>
{
    public RemoveVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
