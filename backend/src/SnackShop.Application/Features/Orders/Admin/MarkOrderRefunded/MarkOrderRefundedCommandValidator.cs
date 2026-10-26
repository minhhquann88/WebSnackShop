using FluentValidation;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.MarkOrderRefunded;

public sealed class MarkOrderRefundedCommandValidator : AbstractValidator<MarkOrderRefundedCommand>
{
    public MarkOrderRefundedCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.TransactionRef).MaximumLength(Payment.MaxTransactionRefLength);
    }
}
