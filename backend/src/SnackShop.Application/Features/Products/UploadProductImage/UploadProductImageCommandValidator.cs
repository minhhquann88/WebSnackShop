using FluentValidation;
using SnackShop.Application.Features.Products.Common;
using SnackShop.Domain.Catalog;

namespace SnackShop.Application.Features.Products.UploadProductImage;

public sealed class UploadProductImageCommandValidator : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.Length).InclusiveBetween(1, ImageFormat.MaxBytes)
            .WithMessage("Image must be between 1 byte and 5 MB.");
        RuleFor(x => x.AltText).MaximumLength(ProductImage.MaxAltTextLength);
    }
}
