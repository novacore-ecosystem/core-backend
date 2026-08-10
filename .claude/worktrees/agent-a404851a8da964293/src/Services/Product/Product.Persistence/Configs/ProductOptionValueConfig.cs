using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductOptionValueConfig : IEntityTypeConfiguration<ProductOptionValue>
{
    public void Configure(EntityTypeBuilder<ProductOptionValue> builder)
    {
        // Table
        builder.ToTable("product_option_values");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductOptionId)
            .IsRequired();

        builder.Property(x => x.ProductOptionValueDefinitionId)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.ProductOption)
            .WithMany(o => o.Values)
            .HasForeignKey(x => x.ProductOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ValueDefinition)
            .WithMany()
            .HasForeignKey(x => x.ProductOptionValueDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ProductOptionId);
        builder.HasIndex(x => new { x.ProductOptionId, x.ProductOptionValueDefinitionId })
            .IsUnique();
        builder.HasIndex(x => new { x.ProductOptionId, x.DisplayOrder });
        builder.HasIndex(x => x.ProductOptionValueDefinitionId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
