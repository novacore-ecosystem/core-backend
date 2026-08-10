using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductBrandTranslationConfig : IEntityTypeConfiguration<ProductBrandTranslation>
{
    public void Configure(EntityTypeBuilder<ProductBrandTranslation> builder)
    {
        // Table
        builder.ToTable("product_brand_translations");

        // Properties
        // Id doubles as the owning Brand's Id (see ProductBrandTranslation.Create) - one row per
        // language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(x => x.Brand)
            .WithMany(b => b.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
