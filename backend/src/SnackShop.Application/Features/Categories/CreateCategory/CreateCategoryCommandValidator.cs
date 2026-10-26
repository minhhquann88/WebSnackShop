using FluentValidation;
using SnackShop.Domain.Catalog;

namespace SnackShop.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Category.MaxNameLength);
        RuleFor(x => x.Slug).MaximumLength(Slug.MaxLength);
        RuleFor(x => x.Description).MaximumLength(Category.MaxDescriptionLength);
        RuleFor(x => x.ImageUrl).MaximumLength(Category.MaxImageUrlLength);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
