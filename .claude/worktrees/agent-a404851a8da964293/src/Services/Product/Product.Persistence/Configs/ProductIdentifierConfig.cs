using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductIdentifierConfig : IEntityTypeConfiguration<ProductIdentifier>
{
    public void Configure(EntityTypeBuilder<ProductIdentifier> builder)
    {
        // Table
        builder.ToTable("product_identifiers");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasConversion(x => x.Value, x => Sku.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Barcode)
            .HasConversion(
                x => x == null ? null : x.Value,
                x => x == null ? null : Barcode.Create(x))
            .HasMaxLength(14);

        builder.Property(x => x.Upc)
            .HasMaxLength(12);

        builder.Property(x => x.Ean)
            .HasMaxLength(13);

        builder.Property(x => x.Isbn)
            .HasMaxLength(13);

        builder.Property(x => x.Gtin)
            .HasMaxLength(14);

        builder.Property(x => x.Mpn)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(x => x.Variant)
            .WithOne(v => v.Identifier)
            .HasForeignKey<ProductIdentifier>(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.VariantId)
            .IsUnique();
        builder.HasIndex(x => x.Sku)
            .IsUnique();
        builder.HasIndex(x => x.Barcode)
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
