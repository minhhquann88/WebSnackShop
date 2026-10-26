using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Orders.Admin.ListAdminOrders;

public sealed class ListAdminOrdersQueryValidator : AbstractValidator<ListAdminOrdersQuery>
{
    public ListAdminOrdersQueryValidator()
    {
        this.ValidatePaging();
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.PaymentStatus).IsInEnum();
        RuleFor(x => x.To).GreaterThan(x => x.From).When(x => x.From is not null && x.To is not null);
    }
}
