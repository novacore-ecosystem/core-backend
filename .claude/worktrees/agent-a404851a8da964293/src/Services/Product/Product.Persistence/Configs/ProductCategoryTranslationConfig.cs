using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductCategoryTranslationConfig : IEntityTypeConfiguration<ProductCategoryTranslation>
{
    public void Configure(EntityTypeBuilder<ProductCategoryTranslation> builder)
    {
        // Table
        builder.ToTable("product_category_translations");

        // Properties
        // Id doubles as the owning Category's Id (see ProductCategoryTranslation.Create) - one
        // row per language, so the primary key must include LanguageCode.
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

        builder.Property(x => x.Note)
            .HasMaxLength(1000)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        // Relationships
        builder.HasOne(x => x.Category)
            .WithMany(c => c.Translation)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
