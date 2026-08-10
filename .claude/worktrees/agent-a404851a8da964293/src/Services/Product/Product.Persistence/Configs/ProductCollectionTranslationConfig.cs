using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductCollectionTranslationConfig : IEntityTypeConfiguration<ProductCollectionTranslation>
{
    public void Configure(EntityTypeBuilder<ProductCollectionTranslation> builder)
    {
        // Table
        builder.ToTable("product_collection_translations");

        // Properties
        // Id doubles as the owning Collection's Id (see ProductCollectionTranslation.Create) -
        // one row per language, so the primary key must include LanguageCode.
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
        builder.HasOne(x => x.Collection)
            .WithMany(c => c.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
