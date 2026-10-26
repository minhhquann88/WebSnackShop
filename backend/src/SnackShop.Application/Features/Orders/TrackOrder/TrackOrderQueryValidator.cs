using FluentValidation;
using SnackShop.Application.Common.Validation;
using SnackShop.Domain.Ordering;

namespace SnackShop.Application.Features.Orders.TrackOrder;

public sealed class TrackOrderQueryValidator : AbstractValidator<TrackOrderQuery>
{
    public TrackOrderQueryValidator()
    {
        RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(OrderNumber.MaxLength);
        RuleFor(x => x.Phone).VietnamesePhone();
    }
}
