using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventoryTransactionConfig : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("inventory_transactions");

        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.InventoryId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.InventoryDocumentId);

        builder.Property(x => x.InventoryReservationId);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Quantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x))
            .IsRequired();

        builder.Property(x => x.BeforeOnHandQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.AfterOnHandQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.BeforeReservedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.AfterReservedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => InventoryTransactionMetadata.FromJson<InventoryTransactionMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder
            .HasOne(x => x.Inventory)
            .WithMany()
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey(x => x.InventoryDocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Reservation)
            .WithMany()
            .HasForeignKey(x => x.InventoryReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes - composite for common queries
        builder.HasIndex(x => new { x.InventoryId, x.CreatedAt });

        builder.HasIndex(x => x.Type);

        builder.HasIndex(x => x.ProductId);

        builder.HasIndex(x => x.WarehouseId);

        // Audit (no concurrency token for immutable ledger)
        builder.ConfigureAuditFields();
    }
}
