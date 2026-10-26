using Microsoft.EntityFrameworkCore;
using SnackShop.Application.Abstractions.Messaging;
using SnackShop.Application.Abstractions.Persistence;
using SnackShop.Application.Common;
using SnackShop.Domain.Common;
using SnackShop.Domain.Settings;

namespace SnackShop.Application.Features.Settings.GetPublicSettings;

public sealed class GetPublicSettingsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetPublicSettingsQuery, PublicSettingsResponse>
{
    public async Task<Result<PublicSettingsResponse>> Handle(GetPublicSettingsQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.ShopSettings
            .AsNoTracking()
            .Select(s => new PublicSettingsResponse(
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
                s.BankBin))
            .FirstOrDefaultAsync(cancellationToken);

        return settings is null
            ? Result.Failure<PublicSettingsResponse>(SettingsErrors.NotSeeded)
            : settings;
    }
}
