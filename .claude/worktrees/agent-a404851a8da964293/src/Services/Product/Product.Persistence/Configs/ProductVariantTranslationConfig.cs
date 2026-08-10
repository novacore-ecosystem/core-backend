using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductVariantTranslationConfig : IEntityTypeConfiguration<ProductVariantTranslation>
{
    public void Configure(EntityTypeBuilder<ProductVariantTranslation> builder)
    {
        // Table
        builder.ToTable("product_variant_translations");

        // Properties
        // Id doubles as the owning Variant's Id (see ProductVariantTranslation.Create) - one row
        // per language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ShortDescription)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.SeoTitle)
            .HasMaxLength(200);

        builder.Property(x => x.SeoDescription)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.Variant)
            .WithMany(v => v.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
