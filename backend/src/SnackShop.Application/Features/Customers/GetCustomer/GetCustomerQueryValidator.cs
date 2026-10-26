using FluentValidation;

namespace SnackShop.Application.Features.Customers.GetCustomer;

public sealed class GetCustomerQueryValidator : AbstractValidator<GetCustomerQuery>
{
    public GetCustomerQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
