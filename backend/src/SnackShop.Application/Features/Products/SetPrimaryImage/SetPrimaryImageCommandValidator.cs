using FluentValidation;

namespace SnackShop.Application.Features.Products.SetPrimaryImage;

public sealed class SetPrimaryImageCommandValidator : AbstractValidator<SetPrimaryImageCommand>
{
    public SetPrimaryImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ImageId).NotEmpty();
    }
}
