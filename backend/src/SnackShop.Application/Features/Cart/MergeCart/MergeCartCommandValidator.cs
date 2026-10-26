using FluentValidation;
using SnackShop.Application.Features.Cart.Common;

namespace SnackShop.Application.Features.Cart.MergeCart;

public sealed class MergeCartCommandValidator : AbstractValidator<MergeCartCommand>
{
    public MergeCartCommandValidator()
    {
        this.AddLinesRules(x => x.Items);
    }
}
