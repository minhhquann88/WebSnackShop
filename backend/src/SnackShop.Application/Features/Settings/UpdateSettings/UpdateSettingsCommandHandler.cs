using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Domain.Common;
using SnackShop.Domain.Settings;

namespace SnackShop.Application.Features.Settings.UpdateSettings;

public sealed class UpdateSettingsCommandHandler(IShopSettingsRepository repository)
    : ICommandHandler<UpdateSettingsCommand>
{
    public async Task<Result> Handle(UpdateSettingsCommand command, CancellationToken cancellationToken)
    {
        var settings = await repository.GetAsync(cancellationToken);
        if (settings is null)
        {
            return Result.Failure(SettingsErrors.NotSeeded);
        }

        return settings.Update(
            command.ShopName,
            command.ShopPhone,
            command.ShopAddress,
            command.FlatShippingFee.ToMoney(),
            command.FreeShippingThreshold?.ToMoney(),
            command.BankName,
            command.BankAccountNumber,
            command.BankAccountName,
            command.BankBin,
            command.UnpaidBankTransferExpiryHours);
    }
}
