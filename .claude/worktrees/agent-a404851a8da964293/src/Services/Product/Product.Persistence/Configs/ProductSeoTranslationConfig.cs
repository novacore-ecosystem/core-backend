using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductSeoTranslationConfig : IEntityTypeConfiguration<ProductSeoTranslation>
{
    public void Configure(EntityTypeBuilder<ProductSeoTranslation> builder)
    {
        // Table
        builder.ToTable("product_seo_translations");

        // Properties
        // Id doubles as the owning Product/ProductSeo Id (see ProductSeoTranslation.Create) -
        // one row per language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.MetaTitle)
            .HasMaxLength(200);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(500);

        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(500);

        // Relationships
        // The actual owning collection (ProductSeo.Translations) is configured from ProductSeo's
        // side with the cascading FK; this direct Product reference shares the same column
        // (Id == Product.Id == ProductSeo.Id) so it is mapped without a second cascade path.
        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.NoAction);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
