using Microsoft.EntityFrameworkCore;
using SnackShop.Domain.Settings;

namespace SnackShop.Infrastructure.Persistence.Repositories;

internal sealed class ShopSettingsRepository(ApplicationDbContext dbContext) : IShopSettingsRepository
{
    public Task<ShopSettings?> GetAsync(CancellationToken cancellationToken) =>
        dbContext.ShopSettingsSet.FirstOrDefaultAsync(s => s.Id == ShopSettings.SingletonId, cancellationToken);
}
