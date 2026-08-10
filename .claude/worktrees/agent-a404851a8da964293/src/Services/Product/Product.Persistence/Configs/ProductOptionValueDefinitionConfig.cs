using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Product.Domain.Metadata;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductOptionValueDefinitionConfig : IEntityTypeConfiguration<ProductOptionValueDefinition>
{
    public void Configure(EntityTypeBuilder<ProductOptionValueDefinition> builder)
    {
        // Table
        builder.ToTable("product_option_value_definitions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductOptionDefinitionId)
            .IsRequired();

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

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<ProductOptionValueDefinitionMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.ProductOptionDefinition)
            .WithMany(d => d.ValueDefinitions)
            .HasForeignKey(x => x.ProductOptionDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.ProductOptionValueDefinition)
            .HasForeignKey(t => t.ProductOptionValueDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProductOptionDefinitionId);
        builder.HasIndex(x => new { x.ProductOptionDefinitionId, x.Code })
            .IsUnique();
        builder.HasIndex(x => new { x.ProductOptionDefinitionId, x.DisplayOrder });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
