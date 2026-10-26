using FluentValidation;
using SnackShop.Application.Common.Validation;
using SnackShop.Application.Features.Cart.Common;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        this.AddLinesRules(x => x.Items);
        RuleFor(x => x.ContactEmail).EmailAddressRule();
        RuleFor(x => x.ShippingAddress).NotNull().SetValidator(new AddressRequestValidator());
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.CustomerNote).MaximumLength(Order.MaxNoteLength);
        RuleFor(x => x.IdempotencyKey).MaximumLength(Order.MaxIdempotencyKeyLength)
            .Matches("^[A-Za-z0-9_-]+$").When(x => x.IdempotencyKey is not null)
            .WithMessage("Idempotency key may contain only letters, digits, '-' and '_'.");
    }
}
