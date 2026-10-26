using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnackShop.Infrastructure.Identity;

namespace SnackShop.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedNever();

        b.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
        b.Property(t => t.ReplacedByTokenHash).HasMaxLength(128);
        b.Property(t => t.CreatedByIp).HasMaxLength(64);
        b.Property(t => t.DeviceLabel).HasMaxLength(200);
        b.Property(t => t.RevokedReason).HasMaxLength(30);

        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("ux_refresh_tokens_token_hash");
        b.HasIndex(t => t.UserId);
        b.HasIndex(t => t.ExpiresAt);
    }
}
