using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Settings.GetAdminSettings;

/// <summary>Full settings row for the admin settings page.</summary>
public sealed record GetAdminSettingsQuery : IQuery<AdminSettingsResponse>;

public sealed record AdminSettingsResponse(
    string ShopName,
    string ShopPhone,
    string ShopAddress,
    MoneyResponse FlatShippingFee,
    MoneyResponse? FreeShippingThreshold,
    string BankName,
    string BankAccountNumber,
    string BankAccountName,
    string BankBin,
    int UnpaidBankTransferExpiryHours,
    DateTimeOffset? UpdatedAt);
