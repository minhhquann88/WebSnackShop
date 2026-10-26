using FluentValidation;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.CancelOrder;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(Order.MaxNoteLength);
    }
}
