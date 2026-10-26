using FluentValidation;

namespace SnackShop.Application.Features.Cart.AddCartItem;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, Domain.Customers.Cart.MaxQuantityPerItem);
    }
}
