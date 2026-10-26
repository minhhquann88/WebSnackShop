using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Settings.GetPublicSettings;

public sealed record PublicSettingsResponse(
    string ShopName,
    string ShopPhone,
    string ShopAddress,
    MoneyResponse FlatShippingFee,
    MoneyResponse? FreeShippingThreshold,
    string BankName,
    string BankAccountNumber,
    string BankAccountName,
    string BankBin);
