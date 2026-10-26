using FluentValidation;
using SnackShop.Domain.Customers;

namespace SnackShop.Application.Common.Validation;

/// <summary>Reusable FluentValidation rules so every feature validates the same things the same way.</summary>
public static class ValidationRules
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;
    public const int MaxEmailLength = 256;

    public static void ValidatePaging<T>(this AbstractValidator<T> validator)
        where T : IPagedQuery
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        validator.RuleFor(x => x.PageSize).InclusiveBetween(1, PagedResponseExtensions.MaxPageSize);
    }

    public static IRuleBuilderOptions<T, string> VietnamesePhone<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Matches(AddressInfo.PhonePattern)
            .WithMessage("Phone must be 10 digits starting with 0.");

    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Length(MinPasswordLength, MaxPasswordLength)
            .Matches("[A-Za-z]").WithMessage("Password must contain a letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");

    public static IRuleBuilderOptions<T, string> EmailAddressRule<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(MaxEmailLength).EmailAddress();
}
