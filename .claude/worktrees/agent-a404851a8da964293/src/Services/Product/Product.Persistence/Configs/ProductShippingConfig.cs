using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Product.Persistence.Configs;

public sealed class ProductShippingConfig : IEntityTypeConfiguration<ProductShipping>
{
    public void Configure(EntityTypeBuilder<ProductShipping> builder)
    {
        // Table
        builder.ToTable("product_shippings");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShippingClass)
            .IsRequired()
            .HasConversion<short>();

        builder.OwnsOne(x => x.Weight, weight =>
        {
            weight.Property(w => w.Value)
                .HasColumnName("weight")
                .HasColumnType("numeric(10,3)");

            weight.Property(w => w.Unit)
                .HasColumnName("weight_unit")
                .HasConversion<short>();
        });

        builder.OwnsOne(x => x.Dimension, dimension =>
        {
            dimension.Property(d => d.Length)
                .HasColumnName("dimension_length")
                .HasColumnType("numeric(10,2)");

            dimension.Property(d => d.Width)
                .HasColumnName("dimension_width")
                .HasColumnType("numeric(10,2)");

            dimension.Property(d => d.Height)
                .HasColumnName("dimension_height")
                .HasColumnType("numeric(10,2)");

            dimension.Property(d => d.Unit)
                .HasColumnName("dimension_unit")
                .HasConversion<short>();
        });

        builder.Property(x => x.RequiresShipping)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.IsFragile)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsHazardous)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AllowBackOrder)
            .IsRequired()
            .HasDefaultValue(false);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
