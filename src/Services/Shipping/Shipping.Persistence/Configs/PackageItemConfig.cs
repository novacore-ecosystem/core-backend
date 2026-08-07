using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class PackageItemConfig : IEntityTypeConfiguration<PackageItem>
{
    public void Configure(EntityTypeBuilder<PackageItem> builder)
    {
        // Table
        builder.ToTable("package_items");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PackageId).IsRequired();
        builder.Property(x => x.ShipmentItemId).IsRequired();
        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // ShipmentItemId is deliberately a plain indexed column, not a configured FK: both Package
        // and ShipmentItem already cascade from Shipment, so a second FK would create a second
        // cascade path to the same root for no integrity gain inside a single aggregate.

        // Indexes
        builder.HasIndex(x => x.PackageId);
        builder.HasIndex(x => x.ShipmentItemId);
        builder.HasIndex(x => new { x.PackageId, x.ShipmentItemId }).IsUnique();
    }
}
