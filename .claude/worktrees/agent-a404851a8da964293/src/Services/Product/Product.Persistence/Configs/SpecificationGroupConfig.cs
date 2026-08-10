using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class SpecificationGroupConfig : IEntityTypeConfiguration<SpecificationGroup>
{
    public void Configure(EntityTypeBuilder<SpecificationGroup> builder)
    {
        // Table
        builder.ToTable("specification_groups");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(CatalogStatus.Active);

        // Relationships
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.SpecificationGroup)
            .HasForeignKey(t => t.SpecificationGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SpecificationDefinitions)
            .WithOne(d => d.Group)
            .HasForeignKey(d => d.SpecificationGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
