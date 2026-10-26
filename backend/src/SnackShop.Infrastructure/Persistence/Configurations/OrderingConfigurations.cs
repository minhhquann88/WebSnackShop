using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnackShop.Domain.Customers;
using SnackShop.Domain.Ordering;

namespace SnackShop.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("orders");
        b.HasKey(o => o.Id);

        b.Property(o => o.OrderNumber)
            .HasConversion(n => n.Value, v => OrderNumber.FromTrusted(v))
            .HasMaxLength(OrderNumber.MaxLength)
            .IsRequired();
        b.HasIndex(o => o.OrderNumber).IsUnique().HasDatabaseName("ux_orders_order_number");

        b.HasOne<Customer>().WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.SetNull);
        b.Property(o => o.ContactEmail).HasMaxLength(Order.MaxEmailLength).IsRequired();

        b.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(x => x.FullName).HasMaxLength(AddressInfo.MaxNameLength).IsRequired();
            a.Property(x => x.Phone).HasMaxLength(AddressInfo.MaxPhoneLength).IsRequired();
            a.Property(x => x.Line1).HasMaxLength(AddressInfo.MaxLineLength).IsRequired();
            a.Property(x => x.Ward).HasMaxLength(AddressInfo.MaxAreaLength).IsRequired();
            a.Property(x => x.District).HasMaxLength(AddressInfo.MaxAreaLength).IsRequired();
            a.Property(x => x.Province).HasMaxLength(AddressInfo.MaxAreaLength).IsRequired();
            a.HasIndex(x => x.Phone);
        });
        b.Navigation(o => o.ShippingAddress).IsRequired();

        b.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        b.Property(o => o.CancelledBy).HasConversion<string>().HasMaxLength(20);

        b.ComplexProperty(o => o.Subtotal, MoneyConfiguration.Configure);
        b.ComplexProperty(o => o.ShippingFee, MoneyConfiguration.Configure);
        b.ComplexProperty(o => o.DiscountAmount, MoneyConfiguration.Configure);
        b.ComplexProperty(o => o.Total, MoneyConfiguration.Configure);

        b.Property(o => o.CouponCode).HasMaxLength(50);
        b.Property(o => o.CustomerNote).HasMaxLength(Order.MaxNoteLength);
        b.Property(o => o.InternalNote).HasMaxLength(Order.MaxInternalNoteLength);
        b.Property(o => o.CancelReason).HasMaxLength(Order.MaxNoteLength);
        b.Property(o => o.IdempotencyKey).HasMaxLength(Order.MaxIdempotencyKeyLength);
        b.HasIndex(o => o.IdempotencyKey).IsUnique().HasFilter("idempotency_key IS NOT NULL");

        b.Property<uint>("Version").IsRowVersion().HasColumnName("xmin").HasColumnType("xid");

        b.HasOne(o => o.Payment).WithOne().HasForeignKey<Payment>(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(o => o.Payment).IsRequired();

        b.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(o => o.StatusHistory).WithOne().HasForeignKey(h => h.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(o => o.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasIndex(o => new { o.CustomerId, o.PlacedAt });
        b.HasIndex(o => new { o.Status, o.PlacedAt });
        b.HasIndex(o => o.PlacedAt);
        b.HasIndex(o => o.DeliveredAt);
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("order_items");
        b.HasKey(i => i.Id);

        // No FK to product_variants on purpose: order lines are snapshots and must survive catalog deletions.
        b.Property(i => i.ProductName).HasMaxLength(OrderItem.MaxProductNameLength).IsRequired();
        b.Property(i => i.VariantName).HasMaxLength(OrderItem.MaxVariantNameLength).IsRequired();
        b.Property(i => i.Sku).HasMaxLength(OrderItem.MaxSkuLength).IsRequired();
        b.Property(i => i.ImageUrl).HasMaxLength(OrderItem.MaxImageUrlLength);
        b.ComplexProperty(i => i.UnitPrice, MoneyConfiguration.Configure);
        b.ComplexProperty(i => i.LineTotal, MoneyConfiguration.Configure);

        b.HasIndex(i => i.VariantId);
        b.HasIndex(i => i.ProductId);
    }
}

internal sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> b)
    {
        b.ToTable("order_status_histories");
        b.HasKey(h => h.Id);

        b.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(20);
        b.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(20);
        b.Property(h => h.ActorType).HasConversion<string>().HasMaxLength(20);
        b.Property(h => h.Note).HasMaxLength(OrderStatusHistory.MaxNoteLength);
    }
}

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("payments");
        b.HasKey(p => p.Id);

        b.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
        b.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        b.ComplexProperty(p => p.Amount, MoneyConfiguration.Configure);
        b.Property(p => p.Provider).HasMaxLength(Payment.MaxProviderLength).IsRequired();
        b.Property(p => p.TransactionRef).HasMaxLength(Payment.MaxTransactionRefLength);
        b.Property(p => p.TransferContent).HasMaxLength(Payment.MaxTransferContentLength);
        b.Ignore(p => p.IsPaid);

        b.HasIndex(p => p.Status);
    }
}
