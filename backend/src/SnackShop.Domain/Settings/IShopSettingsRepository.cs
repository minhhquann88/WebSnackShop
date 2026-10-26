namespace SnackShop.Domain.Settings;

/// <summary>
/// Write-side access to the singleton <see cref="ShopSettings"/> row (tracked, for commands).
/// Read-side queries use IApplicationDbContext directly.
/// </summary>
public interface IShopSettingsRepository
{
    /// <summary>Returns the tracked settings row, or null if the database has not been seeded.</summary>
    Task<ShopSettings?> GetAsync(CancellationToken cancellationToken);
}
