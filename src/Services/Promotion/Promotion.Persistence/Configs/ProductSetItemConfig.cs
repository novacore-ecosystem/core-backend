using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ProductSetItemConfig : IEntityTypeConfiguration<ProductSetItem>
{
    public void Configure(EntityTypeBuilder<ProductSetItem> builder)
    {
        // Table
        builder.ToTable("product_set_items");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("quantity")
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.ProductSet)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ProductSetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProductSetId);
        builder.HasIndex(x => x.ProductId);
    }
}
