using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductOptionValueDefinitionTranslationConfig : IEntityTypeConfiguration<ProductOptionValueDefinitionTranslation>
{
    public void Configure(EntityTypeBuilder<ProductOptionValueDefinitionTranslation> builder)
    {
        // Table
        builder.ToTable("product_option_value_definition_translations");

        // Properties
        builder.HasKey(x => new { x.ProductOptionValueDefinitionId, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.ProductOptionValueDefinition)
            .WithMany(v => v.Translations)
            .HasForeignKey(x => x.ProductOptionValueDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
