using FluentValidation;

namespace SnackShop.Application.Features.Orders.ExpireUnpaidOrders;

public sealed class ExpireUnpaidOrderCommandValidator : AbstractValidator<ExpireUnpaidOrderCommand>
{
    public ExpireUnpaidOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
