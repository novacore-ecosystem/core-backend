using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class WarehouseZoneConfig : IEntityTypeConfiguration<WarehouseZone>
{
    public void Configure(EntityTypeBuilder<WarehouseZone> builder)
    {
        builder.ToTable("warehouse_zones");
        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(WarehouseZoneStatus.Active);

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        // TODO: Configure owned types properly with JSON serialization
        // For now, ignore them to allow initial migration
        builder.Ignore(x => x.Capacity);
        builder.Ignore(x => x.Temperature);
        builder.Ignore(x => x.Humidity);

        builder.Property(x => x.PickingStrategy)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.AllowMixedLot)
            .IsRequired()
            .HasDefaultValue(false);

        builder
            .Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => WarehouseZoneMetadata.FromJson<WarehouseZoneMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Warehouse)
            .WithMany(w => w.Zones)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => new { x.WarehouseId, x.Code })
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Type);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
