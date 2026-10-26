using FluentValidation;

namespace SnackShop.Application.Features.Products.ReorderImages;

public sealed class ReorderImagesCommandValidator : AbstractValidator<ReorderImagesCommand>
{
    public ReorderImagesCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ImageIds).NotEmpty();
    }
}
