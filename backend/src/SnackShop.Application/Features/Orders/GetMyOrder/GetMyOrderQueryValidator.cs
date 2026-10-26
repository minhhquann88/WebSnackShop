using FluentValidation;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.GetMyOrder;

public sealed class GetMyOrderQueryValidator : AbstractValidator<GetMyOrderQuery>
{
    public GetMyOrderQueryValidator()
    {
        RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(OrderNumber.MaxLength);
    }
}
