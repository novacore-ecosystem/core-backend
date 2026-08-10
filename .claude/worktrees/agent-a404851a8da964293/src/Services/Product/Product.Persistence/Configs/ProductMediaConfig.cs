using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductMediaConfig : IEntityTypeConfiguration<ProductMedia>
{
    public void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        // Table
        builder.ToTable("product_media");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.MediaType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Url)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Thumbnail)
            .HasMaxLength(2000);

        builder.Property(x => x.Alt)
            .HasMaxLength(200);

        builder.Property(x => x.Title)
            .HasMaxLength(200);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(CatalogStatus.Active);

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.Media)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Variant)
            .WithMany()
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.VariantId);
        builder.HasIndex(x => new { x.ProductId, x.DisplayOrder });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
