using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Common;

namespace SnackShop.Application.Features.Settings.UpdateSettings;

public sealed record UpdateSettingsCommand(
    string ShopName,
    string ShopPhone,
    string ShopAddress,
    MoneyRequest FlatShippingFee,
    MoneyRequest? FreeShippingThreshold,
    string BankName,
    string BankAccountNumber,
    string BankAccountName,
    string BankBin,
    int UnpaidBankTransferExpiryHours) : ICommand;
