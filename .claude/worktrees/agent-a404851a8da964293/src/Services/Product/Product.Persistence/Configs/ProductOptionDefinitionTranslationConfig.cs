using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductOptionDefinitionTranslationConfig : IEntityTypeConfiguration<ProductOptionDefinitionTranslation>
{
    public void Configure(EntityTypeBuilder<ProductOptionDefinitionTranslation> builder)
    {
        // Table
        builder.ToTable("product_option_definition_translations");

        // Properties
        builder.HasKey(x => new { x.ProductOptionDefinitionId, x.LanguageCode });

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
        builder.HasOne(x => x.ProductOptionDefinition)
            .WithMany(d => d.Translations)
            .HasForeignKey(x => x.ProductOptionDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
