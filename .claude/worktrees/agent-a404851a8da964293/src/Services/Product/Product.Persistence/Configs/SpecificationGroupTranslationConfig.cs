using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class SpecificationGroupTranslationConfig : IEntityTypeConfiguration<SpecificationGroupTranslation>
{
    public void Configure(EntityTypeBuilder<SpecificationGroupTranslation> builder)
    {
        // Table
        builder.ToTable("specification_group_translations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SpecificationGroupId)
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
        builder.HasOne(x => x.SpecificationGroup)
            .WithMany(g => g.Translations)
            .HasForeignKey(x => x.SpecificationGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.SpecificationGroupId, x.LanguageCode })
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
