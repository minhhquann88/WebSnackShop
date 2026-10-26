using FluentValidation;

namespace SnackShop.Application.Features.Orders.ExpireUnpaidOrders;

public sealed class GetExpiredUnpaidOrderIdsQueryValidator : AbstractValidator<GetExpiredUnpaidOrderIdsQuery>
{
    public GetExpiredUnpaidOrderIdsQueryValidator()
    {
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 500);
    }
}
