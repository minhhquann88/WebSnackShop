using FluentValidation;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.Admin.MarkOrderPaid;

public sealed class MarkOrderPaidCommandValidator : AbstractValidator<MarkOrderPaidCommand>
{
    public MarkOrderPaidCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.TransactionRef).MaximumLength(Payment.MaxTransactionRefLength);
        RuleFor(x => x.PaidAt).LessThanOrEqualTo(_ => timeProvider.GetUtcNow().AddMinutes(5))
            .When(x => x.PaidAt is not null).WithMessage("Paid time cannot be in the future.");
    }
}
