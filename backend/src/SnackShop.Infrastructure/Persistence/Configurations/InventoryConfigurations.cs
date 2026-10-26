using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnackShop.Domain.Inventory;

namespace SnackShop.Infrastructure.Persistence.Configurations;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> b)
    {
        b.ToTable("stock_items", t =>
        {
            t.HasCheckConstraint("ck_stock_items_on_hand_non_negative", "quantity_on_hand >= 0");
            t.HasCheckConstraint("ck_stock_items_reserved_within_on_hand", "quantity_reserved >= 0 AND quantity_reserved <= quantity_on_hand");
        });
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).HasColumnName("variant_id");

        b.Ignore(s => s.VariantId);
        b.Ignore(s => s.Available);
        b.Ignore(s => s.IsLowStock);

        // Optimistic concurrency on PostgreSQL's system column (two staff/checkouts touching the same stock).
        b.Property<uint>("Version").IsRowVersion().HasColumnName("xmin").HasColumnType("xid");

        b.HasMany(s => s.Movements).WithOne().HasForeignKey(m => m.VariantId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(s => s.Movements).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> b)
    {
        b.ToTable("stock_movements");
        b.HasKey(m => m.Id);

        b.Property(m => m.Type).HasConversion<string>().HasMaxLength(30);
        b.Property(m => m.ReferenceType).HasMaxLength(30);
        b.Property(m => m.Note).HasMaxLength(StockMovement.MaxNoteLength);

        b.HasIndex(m => new { m.VariantId, m.CreatedAt });
        b.HasIndex(m => new { m.ReferenceType, m.ReferenceId });
    }
}
