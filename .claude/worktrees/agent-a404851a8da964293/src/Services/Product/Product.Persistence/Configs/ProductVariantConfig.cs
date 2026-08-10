using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Product.Domain.Metadata;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductVariantConfig : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        // Table
        builder.ToTable("product_variants");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.VariantType)
            .IsRequired()
            .HasConversion<byte>()
            .HasDefaultValue(ProductVariantType.Simple);

        builder.OwnsOne(x => x.Weight, weight =>
        {
            weight.Property(w => w.Value)
                .HasColumnName("weight")
                .HasColumnType("numeric(10,3)");

            weight.Property(w => w.Unit)
                .HasColumnName("weight_unit")
                .HasConversion<short>();
        });

        builder.Property(x => x.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<VariantMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // IdentifierId mirrors ProductIdentifier.VariantId (the FK below lives on
        // ProductIdentifier's side) - mapped as a plain column, not a second FK.
        builder.Property(x => x.IdentifierId);

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Identifier)
            .WithOne(i => i.Variant)
            .HasForeignKey<ProductIdentifier>(i => i.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Shipping)
            .WithMany()
            .HasForeignKey(x => x.ProductShippingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WarrantyPolicy)
            .WithMany()
            .HasForeignKey(x => x.WarrantyPolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SelectedOptionValues)
            .WithOne(v => v.Variant)
            .HasForeignKey(v => v.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.BundleItems)
            .WithOne(b => b.BundleVariant)
            .HasForeignKey(b => b.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Variant)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => new { x.ProductId, x.DisplayOrder });
        builder.HasIndex(x => x.ProductShippingId);
        builder.HasIndex(x => x.WarrantyPolicyId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
