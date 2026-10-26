using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Orders.ListMyOrders;

public sealed class ListMyOrdersQueryValidator : AbstractValidator<ListMyOrdersQuery>
{
    public ListMyOrdersQueryValidator()
    {
        this.ValidatePaging();
    }
}
