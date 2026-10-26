using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnackShop.Domain.Customers;
using SnackShop.Infrastructure.Identity;

namespace SnackShop.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("customers");
        b.HasKey(c => c.Id);

        b.Property(c => c.FullName).HasMaxLength(Customer.MaxNameLength).IsRequired();
        b.Property(c => c.Email).HasMaxLength(Customer.MaxEmailLength).IsRequired();
        b.Property(c => c.Phone).HasMaxLength(AddressInfo.MaxPhoneLength);

        // The customer profile shares its primary key with the identity account.
        b.HasOne<ApplicationUser>().WithOne().HasForeignKey<Customer>(c => c.Id).OnDelete(DeleteBehavior.Cascade);

        b.HasMany(c => c.Addresses).WithOne().HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(c => c.Addresses).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasIndex(c => c.Email);
        b.HasIndex(c => c.Phone);
    }
}

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> b)
    {
        b.ToTable("addresses");
        b.HasKey(a => a.Id);

        b.Property(a => a.Label).HasMaxLength(Address.MaxLabelLength);
        b.Property(a => a.FullName).HasMaxLength(AddressInfo.MaxNameLength).IsRequired();
        b.Property(a => a.Phone).HasMaxLength(AddressInfo.MaxPhoneLength).IsRequired();
        b.Property(a => a.Line1).HasMaxLength(AddressInfo.MaxLineLength).IsRequired();
        b.Property(a => a.Ward).HasMaxLength(AddressInfo.MaxAreaLength).IsRequired();
        b.Property(a => a.District).HasMaxLength(AddressInfo.MaxAreaLength).IsRequired();
        b.Property(a => a.Province).HasMaxLength(AddressInfo.MaxAreaLength).IsRequired();
    }
}

internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> b)
    {
        b.ToTable("carts");
        b.HasKey(c => c.Id);

        b.HasOne<Customer>().WithOne().HasForeignKey<Cart>(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(c => c.CustomerId).IsUnique();
        b.Property<uint>("Version").IsRowVersion().HasColumnName("xmin").HasColumnType("xid");

        b.HasMany(c => c.Items).WithOne().HasForeignKey(i => i.CartId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(c => c.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> b)
    {
        b.ToTable("cart_items");
        b.HasKey(i => i.Id);
        b.HasIndex(i => new { i.CartId, i.VariantId }).IsUnique();
    }
}
