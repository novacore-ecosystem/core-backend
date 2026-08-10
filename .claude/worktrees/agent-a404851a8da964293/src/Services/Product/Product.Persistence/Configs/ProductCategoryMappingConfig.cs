using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductCategoryMappingConfig : IEntityTypeConfiguration<ProductCategoryMapping>
{
    public void Configure(EntityTypeBuilder<ProductCategoryMapping> builder)
    {
        // Table
        builder.ToTable("product_category_mappings");

        // Properties
        builder.HasKey(x => new { x.ProductId, x.CategoryId });

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.CategoryMappings)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CategoryId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
