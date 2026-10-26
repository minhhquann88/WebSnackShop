using FluentValidation;

namespace SnackShop.Application.Features.Cart.UpdateCartItem;

public sealed class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, Domain.Customers.Cart.MaxQuantityPerItem);
    }
}
