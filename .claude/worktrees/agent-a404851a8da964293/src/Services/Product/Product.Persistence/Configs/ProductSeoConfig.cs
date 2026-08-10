using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductSeoConfig : IEntityTypeConfiguration<ProductSeo>
{
    public void Configure(EntityTypeBuilder<ProductSeo> builder)
    {
        // Table
        builder.ToTable("product_seo");

        // Properties
        // Shared primary key with Product (see ProductSeo.Create: Id = productId) - a strict
        // 1:1 dependent, not an independently identified row.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MetaTitle)
            .HasMaxLength(200);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(500);

        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(500);

        builder.Property(x => x.CanonicalUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.OgTitle)
            .HasMaxLength(200);

        builder.Property(x => x.OgDescription)
            .HasMaxLength(500);

        builder.Property(x => x.OgImage)
            .HasMaxLength(2000);

        // Relationships
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
