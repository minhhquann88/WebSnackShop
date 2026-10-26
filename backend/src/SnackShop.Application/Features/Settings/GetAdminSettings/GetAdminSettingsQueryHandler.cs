using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Settings;

namespace SnackShop.Application.Features.Settings.GetAdminSettings;

public sealed class GetAdminSettingsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetAdminSettingsQuery, AdminSettingsResponse>
{
    public async Task<Result<AdminSettingsResponse>> Handle(GetAdminSettingsQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.ShopSettings
            .AsNoTracking()
            .Select(s => new AdminSettingsResponse(
                s.ShopName,
                s.ShopPhone,
                s.ShopAddress,
                new MoneyResponse(s.FlatShippingFee.Amount, s.FlatShippingFee.Currency),
                s.FreeShippingThreshold == null
                    ? null
                    : new MoneyResponse(s.FreeShippingThreshold.Amount, s.FreeShippingThreshold.Currency),
                s.BankName,
                s.BankAccountNumber,
                s.BankAccountName,
                s.BankBin,
                s.UnpaidBankTransferExpiryHours,
                s.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return settings is null
            ? Result.Failure<AdminSettingsResponse>(SettingsErrors.NotSeeded)
            : settings;
    }
}
