using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Product.Domain.Metadata;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductOptionDefinitionConfig : IEntityTypeConfiguration<ProductOptionDefinition>
{
    public void Configure(EntityTypeBuilder<ProductOptionDefinition> builder)
    {
        // Table
        builder.ToTable("product_option_definitions");

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

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => ProductOptionDefinitionMetadata.FromJson<ProductOptionDefinitionMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasMany(x => x.ValueDefinitions)
            .WithOne(v => v.ProductOptionDefinition)
            .HasForeignKey(v => v.ProductOptionDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.ProductOptionDefinition)
            .HasForeignKey(t => t.ProductOptionDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.DisplayOrder);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
