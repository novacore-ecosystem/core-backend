using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class VariantOptionValueConfig : IEntityTypeConfiguration<VariantOptionValue>
{
    public void Configure(EntityTypeBuilder<VariantOptionValue> builder)
    {
        // Table
        builder.ToTable("variant_option_values");

        // Properties
        builder.HasKey(x => new { x.VariantId, x.ProductOptionValueId });

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Variant)
            .WithMany(v => v.SelectedOptionValues)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProductOptionValue)
            .WithMany()
            .HasForeignKey(x => x.ProductOptionValueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ProductOptionValueId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
