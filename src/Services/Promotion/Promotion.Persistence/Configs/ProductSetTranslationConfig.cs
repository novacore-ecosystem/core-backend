using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ProductSetTranslationConfig : IEntityTypeConfiguration<ProductSetTranslation>
{
    public void Configure(EntityTypeBuilder<ProductSetTranslation> builder)
    {
        // Table
        builder.ToTable("product_set_translations");

        // Properties
        // Identity is ProductSetId + LanguageCode - no surrogate Id (Phase 3.1 Translation policy).
        builder.HasKey(x => new { x.ProductSetId, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.ProductSet)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.ProductSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
