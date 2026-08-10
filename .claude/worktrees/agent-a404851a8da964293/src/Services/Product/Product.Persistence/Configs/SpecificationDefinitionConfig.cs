using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class SpecificationDefinitionConfig : IEntityTypeConfiguration<SpecificationDefinition>
{
    public void Configure(EntityTypeBuilder<SpecificationDefinition> builder)
    {
        // Table
        builder.ToTable("specification_definitions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SpecificationGroupId)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DataType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.DefaultUnit)
            .HasMaxLength(20);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(CatalogStatus.Active);

        // Relationships
        builder.HasOne(x => x.Group)
            .WithMany(g => g.SpecificationDefinitions)
            .HasForeignKey(x => x.SpecificationGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.SpecificationDefinition)
            .HasForeignKey(t => t.SpecificationDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.SpecificationGroupId);
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
