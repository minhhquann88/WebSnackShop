using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnackShop.Domain.Catalog;

namespace SnackShop.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("categories");
        b.HasKey(c => c.Id);

        b.Property(c => c.Name).HasMaxLength(Category.MaxNameLength).IsRequired();
        b.Property(c => c.Slug)
            .HasConversion(s => s.Value, v => Slug.FromTrusted(v))
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();
        b.Property(c => c.Description).HasMaxLength(Category.MaxDescriptionLength);
        b.Property(c => c.ImageUrl).HasMaxLength(Category.MaxImageUrlLength);

        b.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("ux_categories_slug");
        b.HasIndex(c => c.Name).IsUnique().HasDatabaseName("ux_categories_name");
        b.HasQueryFilter(c => !c.IsDeleted);
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("products");
        b.HasKey(p => p.Id);

        b.Property(p => p.Name).HasMaxLength(Product.MaxNameLength).IsRequired();
        b.Property(p => p.Slug)
            .HasConversion(s => s.Value, v => Slug.FromTrusted(v))
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();
        b.Property(p => p.ShortDescription).HasMaxLength(Product.MaxShortDescriptionLength);
        b.Property(p => p.Description).HasMaxLength(Product.MaxDescriptionLength);
        b.Property(p => p.Ingredients).HasMaxLength(Product.MaxInfoFieldLength);
        b.Property(p => p.Origin).HasMaxLength(Product.MaxInfoFieldLength);
        b.Property(p => p.Brand).HasMaxLength(Product.MaxInfoFieldLength);
        b.Property(p => p.SearchText).HasMaxLength(Product.MaxNameLength + Product.MaxInfoFieldLength + 1).IsRequired();

        b.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(p => p.Variants).WithOne().HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(p => p.Images).WithOne().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(p => p.Variants).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);
        b.Navigation(p => p.Images).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude(false);

        b.HasIndex(p => p.Slug).IsUnique().HasDatabaseName("ux_products_slug");
        b.HasIndex(p => new { p.CategoryId, p.IsActive });
        b.HasIndex(p => p.CreatedAt);
        b.HasQueryFilter(p => !p.IsDeleted);

        // Concurrent deliveries increment SoldCount; an admin edit racing with them retries instead of losing data.
        b.Property<uint>("Version").IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
    }
}

internal sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> b)
    {
        b.ToTable("product_variants");
        b.HasKey(v => v.Id);

        b.Property(v => v.Name).HasMaxLength(ProductVariant.MaxNameLength).IsRequired();
        b.Property(v => v.Sku)
            .HasConversion(s => s.Value, v => Sku.FromTrusted(v))
            .HasMaxLength(Sku.MaxLength)
            .IsRequired();
        b.ComplexProperty(v => v.Price, MoneyConfiguration.Configure);
        b.OwnsOne(v => v.CompareAtPrice, m => MoneyConfiguration.ConfigureOptional(m, "compare_at_price"));

        b.HasIndex(v => v.Sku).IsUnique().HasDatabaseName("ux_product_variants_sku");
    }
}

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> b)
    {
        b.ToTable("product_images");
        b.HasKey(i => i.Id);

        b.Property(i => i.Url).HasMaxLength(ProductImage.MaxUrlLength).IsRequired();
        b.Property(i => i.AltText).HasMaxLength(ProductImage.MaxAltTextLength);
    }
}
