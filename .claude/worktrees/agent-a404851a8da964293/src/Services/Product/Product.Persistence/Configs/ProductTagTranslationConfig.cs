using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductTagTranslationConfig : IEntityTypeConfiguration<ProductTagTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTagTranslation> builder)
    {
        // Table
        builder.ToTable("product_tag_translations");

        // Properties
        // Id doubles as the owning Tag's Id (see ProductTagTranslation.Create) - one row per
        // language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.ProductTag)
            .WithMany(t => t.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
