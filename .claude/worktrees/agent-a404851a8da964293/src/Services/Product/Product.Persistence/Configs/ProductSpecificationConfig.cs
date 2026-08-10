using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductSpecificationConfig : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        // Table
        builder.ToTable("product_specifications");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.SpecificationDefinitionId)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.Specifications)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SpecificationDefinition)
            .WithMany()
            .HasForeignKey(x => x.SpecificationDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => new { x.ProductId, x.SpecificationDefinitionId })
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
