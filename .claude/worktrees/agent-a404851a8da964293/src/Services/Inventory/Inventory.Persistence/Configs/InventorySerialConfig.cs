using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventorySerialConfig : IEntityTypeConfiguration<InventorySerial>
{
    public void Configure(EntityTypeBuilder<InventorySerial> builder)
    {
        builder.ToTable("inventory_serials");
        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.InventoryId).IsRequired();

        builder.Property(x => x.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(InventorySerialStatus.Available);

        builder.Property(x => x.InventoryReservationId);
        builder.Property(x => x.InventoryDocumentId);

        builder
            .Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => InventorySerialMetadata.FromJson<InventorySerialMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Inventory)
            .WithMany()
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(x => x.Reservation)
            .WithMany()
            .HasForeignKey(x => x.InventoryReservationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey(x => x.InventoryDocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.InventoryId);
        builder.HasIndex(x => x.SerialNumber)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.InventoryReservationId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
