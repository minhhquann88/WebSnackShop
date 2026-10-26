using FluentValidation;
using SnackShop.Application.Common.Validation;

namespace SnackShop.Application.Features.Customers.ListCustomers;

public sealed class ListCustomersQueryValidator : AbstractValidator<ListCustomersQuery>
{
    public ListCustomersQueryValidator()
    {
        this.ValidatePaging();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
