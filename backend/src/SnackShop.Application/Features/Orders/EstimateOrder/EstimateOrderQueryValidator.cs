using FluentValidation;
using SnackShop.Application.Features.Cart.Common;

namespace SnackShop.Application.Features.Orders.EstimateOrder;

public sealed class EstimateOrderQueryValidator : AbstractValidator<EstimateOrderQuery>
{
    public EstimateOrderQueryValidator()
    {
        this.AddLinesRules(x => x.Items);
    }
}
