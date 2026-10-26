using FluentValidation;

namespace SnackShop.Application.Features.Orders.Admin.StartPreparingOrder;

public sealed class StartPreparingOrderCommandValidator : AbstractValidator<StartPreparingOrderCommand>
{
    public StartPreparingOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
