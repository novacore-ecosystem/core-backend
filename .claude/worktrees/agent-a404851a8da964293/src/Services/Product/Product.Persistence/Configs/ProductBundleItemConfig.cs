using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductBundleItemConfig : IEntityTypeConfiguration<ProductBundleItem>
{
    public void Configure(EntityTypeBuilder<ProductBundleItem> builder)
    {
        // Table
        builder.ToTable("product_bundle_items");

        // Properties
        // Id doubles as the owning (Bundle) Variant's Id (see ProductBundleItem.Create) - a
        // bundle has multiple items, so the primary key must include VariantId (the component).
        builder.HasKey(x => new { x.Id, x.VariantId });

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.BundleVariant)
            .WithMany(v => v.BundleItems)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Variant)
            .WithMany()
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.VariantId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
