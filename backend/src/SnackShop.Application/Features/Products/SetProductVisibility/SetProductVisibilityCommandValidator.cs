using FluentValidation;

namespace SnackShop.Application.Features.Products.SetProductVisibility;

public sealed class SetProductVisibilityCommandValidator : AbstractValidator<SetProductVisibilityCommand>
{
    public SetProductVisibilityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
