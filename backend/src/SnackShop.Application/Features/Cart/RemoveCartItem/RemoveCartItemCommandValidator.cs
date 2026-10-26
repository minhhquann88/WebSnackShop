using FluentValidation;

namespace SnackShop.Application.Features.Cart.RemoveCartItem;

public sealed class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
