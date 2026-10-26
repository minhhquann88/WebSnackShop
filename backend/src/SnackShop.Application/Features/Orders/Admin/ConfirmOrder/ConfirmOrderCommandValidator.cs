using FluentValidation;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.ConfirmOrder;

public sealed class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.TransactionRef).MaximumLength(Payment.MaxTransactionRefLength);
    }
}
