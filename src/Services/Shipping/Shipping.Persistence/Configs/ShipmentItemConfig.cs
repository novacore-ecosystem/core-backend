using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class ShipmentItemConfig : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        // Table
        builder.ToTable("shipment_items");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShipmentId).IsRequired();
        builder.Property(x => x.LineNo).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(100);
        builder.Property(x => x.WeightKg).HasColumnType("numeric(10,3)");
        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .IsRequired();

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.ShipmentId);
        builder.HasIndex(x => new { x.ShipmentId, x.LineNo }).IsUnique();
    }
}
