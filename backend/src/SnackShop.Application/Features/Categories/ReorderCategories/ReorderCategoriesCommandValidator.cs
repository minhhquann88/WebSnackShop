using FluentValidation;

namespace SnackShop.Application.Features.Categories.ReorderCategories;

public sealed class ReorderCategoriesCommandValidator : AbstractValidator<ReorderCategoriesCommand>
{
    public ReorderCategoriesCommandValidator()
    {
        RuleFor(x => x.CategoryIds).NotEmpty()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Category ids must be unique.");
    }
}
