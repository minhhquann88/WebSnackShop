using FluentValidation;
using SnackShop.Application.Common.Validation;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Features.Me.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(Customer.MaxNameLength);
        RuleFor(x => x.PhoneNumber!).VietnamesePhone().When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
