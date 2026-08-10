using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class SpecificationDefinitionTranslationConfig : IEntityTypeConfiguration<SpecificationDefinitionTranslation>
{
    public void Configure(EntityTypeBuilder<SpecificationDefinitionTranslation> builder)
    {
        // Table
        builder.ToTable("specification_definition_translations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SpecificationDefinitionId)
            .IsRequired();

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
        builder.HasOne(x => x.SpecificationDefinition)
            .WithMany(d => d.Translations)
            .HasForeignKey(x => x.SpecificationDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.SpecificationDefinitionId, x.LanguageCode })
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
