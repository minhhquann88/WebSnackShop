using FluentValidation;
using SnackShop.Domain.Catalog;

namespace SnackShop.Application.Features.Products.Common;

/// <summary>Length rules shared by create and update product commands.</summary>
internal static class ProductFieldsValidation
{
    public static void AddProductFieldRules<T>(
        this AbstractValidator<T> v,
        Func<T, string> name,
        Func<T, string?> slug,
        Func<T, string?> shortDescription,
        Func<T, string?> description,
        Func<T, string?> ingredients,
        Func<T, string?> origin,
        Func<T, string?> brand,
        Func<T, int?> shelfLifeDays)
    {
        v.RuleFor(x => name(x)).NotEmpty().MaximumLength(Product.MaxNameLength).OverridePropertyName("name");
        v.RuleFor(x => slug(x)).MaximumLength(Slug.MaxLength).OverridePropertyName("slug");
        v.RuleFor(x => shortDescription(x)).MaximumLength(Product.MaxShortDescriptionLength).OverridePropertyName("shortDescription");
        v.RuleFor(x => description(x)).MaximumLength(Product.MaxDescriptionLength).OverridePropertyName("description");
        v.RuleFor(x => ingredients(x)).MaximumLength(Product.MaxInfoFieldLength).OverridePropertyName("ingredients");
        v.RuleFor(x => origin(x)).MaximumLength(Product.MaxInfoFieldLength).OverridePropertyName("origin");
        v.RuleFor(x => brand(x)).MaximumLength(Product.MaxInfoFieldLength).OverridePropertyName("brand");
        v.RuleFor(x => shelfLifeDays(x)).InclusiveBetween(1, 3650).When(x => shelfLifeDays(x) is not null).OverridePropertyName("shelfLifeDays");
    }
}
