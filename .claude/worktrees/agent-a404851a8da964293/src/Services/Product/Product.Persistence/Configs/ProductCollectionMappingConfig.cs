using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductCollectionMappingConfig : IEntityTypeConfiguration<ProductCollectionMapping>
{
    public void Configure(EntityTypeBuilder<ProductCollectionMapping> builder)
    {
        // Table
        builder.ToTable("product_collection_mappings");

        // Properties
        builder.HasKey(x => new { x.ProductId, x.CollectionId });

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.CollectionMappings)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Collection)
            .WithMany()
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CollectionId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
