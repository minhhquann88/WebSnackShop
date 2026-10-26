using FluentValidation;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.CancelMyOrder;

public sealed class CancelMyOrderCommandValidator : AbstractValidator<CancelMyOrderCommand>
{
    public CancelMyOrderCommandValidator()
    {
        RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(OrderNumber.MaxLength);
        RuleFor(x => x.Reason).MaximumLength(Order.MaxNoteLength);
    }
}
