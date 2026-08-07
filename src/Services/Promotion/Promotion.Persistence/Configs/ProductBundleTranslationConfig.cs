using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ProductBundleTranslationConfig : IEntityTypeConfiguration<ProductBundleTranslation>
{
    public void Configure(EntityTypeBuilder<ProductBundleTranslation> builder)
    {
        // Table
        builder.ToTable("product_bundle_translations");

        // Properties
        // Identity is BundleId + LanguageCode - no surrogate Id (Phase 3.1 Translation policy).
        builder.HasKey(x => new { x.BundleId, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Bundle)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
