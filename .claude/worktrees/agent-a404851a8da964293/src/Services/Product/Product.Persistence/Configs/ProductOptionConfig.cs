using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductOptionConfig : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(EntityTypeBuilder<ProductOption> builder)
    {
        // Table
        builder.ToTable("product_options");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.ProductOptionDefinitionId)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(CatalogStatus.Active);

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(p => p.Options)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.OptionDefinition)
            .WithMany()
            .HasForeignKey(x => x.ProductOptionDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Values)
            .WithOne(v => v.ProductOption)
            .HasForeignKey(v => v.ProductOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => new { x.ProductId, x.ProductOptionDefinitionId })
            .IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.DisplayOrder });
        builder.HasIndex(x => x.ProductOptionDefinitionId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
