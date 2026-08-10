using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventoryReservationConfig : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.ToTable("inventory_reservations");
        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(InventoryReservationStatus.Pending);

        builder.Property(x => x.InventoryId).IsRequired();
        builder.Property(x => x.WarehouseId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.VariantId).IsRequired();

        builder.Property(x => x.ReferenceType)
            .HasConversion<short?>();

        builder.Property(x => x.ReferenceId);

        builder.Property(x => x.ExternalReference)
            .HasMaxLength(255)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Quantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.ExpiredAt);

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder
            .Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => InventoryReservationMetadata.FromJson<InventoryReservationMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Inventory)
            .WithMany()
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.InventoryId);
        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        builder.HasIndex(x => x.ExpiredAt);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
