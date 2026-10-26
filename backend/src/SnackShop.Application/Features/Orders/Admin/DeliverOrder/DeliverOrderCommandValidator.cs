using FluentValidation;

namespace SnackShop.Application.Features.Orders.Admin.DeliverOrder;

public sealed class DeliverOrderCommandValidator : AbstractValidator<DeliverOrderCommand>
{
    public DeliverOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
