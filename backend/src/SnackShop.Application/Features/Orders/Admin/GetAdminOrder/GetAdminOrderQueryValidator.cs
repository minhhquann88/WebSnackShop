using FluentValidation;

namespace SnackShop.Application.Features.Orders.Admin.GetAdminOrder;

public sealed class GetAdminOrderQueryValidator : AbstractValidator<GetAdminOrderQuery>
{
    public GetAdminOrderQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
