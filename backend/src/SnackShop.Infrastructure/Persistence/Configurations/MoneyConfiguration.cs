using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnackShop.Domain.Common;

namespace SnackShop.Infrastructure.Persistence.Configurations;

/// <summary>
/// Shared mapping for <see cref="Money"/>: numeric(18,2) + varchar(3).
/// Required money uses a complex property (columns {property}_amount / {property}_currency via the naming convention);
/// optional money uses an owned type with explicit column names.
/// </summary>
internal static class MoneyConfiguration
{
    public static void Configure(ComplexPropertyBuilder<Money> money)
    {
        money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
        money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
    }

    public static void ConfigureOptional<TOwner>(OwnedNavigationBuilder<TOwner, Money> money, string columnPrefix)
        where TOwner : class
    {
        money.Property(m => m.Amount).HasColumnName(columnPrefix + "_amount").HasPrecision(18, 2);
        money.Property(m => m.Currency).HasColumnName(columnPrefix + "_currency").HasMaxLength(3);
    }
}
