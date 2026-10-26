using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnackShop.Domain.Settings;

namespace SnackShop.Infrastructure.Persistence.Configurations;

internal sealed class ShopSettingsConfiguration : IEntityTypeConfiguration<ShopSettings>
{
    public void Configure(EntityTypeBuilder<ShopSettings> b)
    {
        b.ToTable("shop_settings");
        b.HasKey(s => s.Id);

        b.Property(s => s.ShopName).HasMaxLength(ShopSettings.MaxNameLength).IsRequired();
        b.Property(s => s.ShopPhone).HasMaxLength(ShopSettings.MaxPhoneLength).IsRequired();
        b.Property(s => s.ShopAddress).HasMaxLength(ShopSettings.MaxAddressLength).IsRequired();

        b.ComplexProperty(s => s.FlatShippingFee, MoneyConfiguration.Configure);
        b.OwnsOne(s => s.FreeShippingThreshold, m => MoneyConfiguration.ConfigureOptional(m, "free_shipping_threshold"));

        b.Property(s => s.BankName).HasMaxLength(ShopSettings.MaxBankFieldLength).IsRequired();
        b.Property(s => s.BankAccountNumber).HasMaxLength(ShopSettings.MaxBankFieldLength).IsRequired();
        b.Property(s => s.BankAccountName).HasMaxLength(ShopSettings.MaxBankFieldLength).IsRequired();
        b.Property(s => s.BankBin).HasMaxLength(10).IsRequired();
        b.Property(s => s.UnpaidBankTransferExpiryHours).IsRequired();
    }
}
