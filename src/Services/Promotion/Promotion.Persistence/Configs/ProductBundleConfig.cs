using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ProductBundleConfig : IEntityTypeConfiguration<ProductBundle>
{
    public void Configure(EntityTypeBuilder<ProductBundle> builder)
    {
        // Table
        builder.ToTable("product_bundles");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.ProductSet)
            .WithMany(x => x.Bundles)
            .HasForeignKey(x => x.ProductSetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prices/Rules/Gifts are all configured from the child entity's own config (single
        // source per relationship).

        // Indexes
        builder.HasIndex(x => x.ProductSetId);
    }
}
