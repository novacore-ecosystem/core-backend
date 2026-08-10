using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductTagMappingConfig : IEntityTypeConfiguration<ProductTagMapping>
{
    public void Configure(EntityTypeBuilder<ProductTagMapping> builder)
    {
        // Table
        builder.ToTable("product_tag_mappings");

        // Properties
        builder.HasKey(x => new { x.ProductId, x.TagId });

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.TagMappings)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.TagId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
