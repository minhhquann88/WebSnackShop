using FluentValidation;
using SnackShop.Application.Common.Validation;
using SnackShop.Domain.Settings;

namespace SnackShop.Application.Features.Settings.UpdateSettings;

public sealed class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsCommandValidator()
    {
        RuleFor(x => x.ShopName).NotEmpty().MaximumLength(ShopSettings.MaxNameLength);
        RuleFor(x => x.ShopPhone).NotEmpty().MaximumLength(ShopSettings.MaxPhoneLength).Matches(@"^0\d{9,10}$")
            .WithMessage("Phone must start with 0 and contain 10 or 11 digits.");
        RuleFor(x => x.ShopAddress).NotEmpty().MaximumLength(ShopSettings.MaxAddressLength);

        RuleFor(x => x.FlatShippingFee).NotNull().SetValidator(new MoneyRequestValidator());
        RuleFor(x => x.FreeShippingThreshold!).SetValidator(new MoneyRequestValidator())
            .When(x => x.FreeShippingThreshold is not null);

        RuleFor(x => x.BankName).NotEmpty().MaximumLength(ShopSettings.MaxBankFieldLength);
        RuleFor(x => x.BankAccountNumber).NotEmpty().MaximumLength(ShopSettings.MaxBankFieldLength).Matches(@"^\d{6,20}$")
            .WithMessage("Bank account number must be 6 to 20 digits.");
        RuleFor(x => x.BankAccountName).NotEmpty().MaximumLength(ShopSettings.MaxBankFieldLength);
        RuleFor(x => x.BankBin).NotEmpty().Matches(@"^\d{6}$").WithMessage("Bank BIN must be exactly 6 digits.");

        RuleFor(x => x.UnpaidBankTransferExpiryHours)
            .InclusiveBetween(ShopSettings.MinUnpaidExpiryHours, ShopSettings.MaxUnpaidExpiryHours);
    }
}
