using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventoryStockConfig : IEntityTypeConfiguration<InventoryStock>
{
    public void Configure(EntityTypeBuilder<InventoryStock> builder)
    {
        builder.ToTable("inventory_stocks");

        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasConversion(
                x => x.Value,
                x => Sku.Create(x))
            .HasColumnName("sku")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(InventoryStatus.Active);

        builder.Property(x => x.OnHandQuantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.ReservedQuantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.IncomingQuantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.OutgoingQuantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.DamagedQuantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.SafetyStock)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.ReorderPoint)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.MaximumStock)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => InventoryMetadata.FromJson<InventoryMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => new { x.VariantId, x.WarehouseId })
            .IsUnique();

        builder.HasIndex(x => x.ProductId);

        builder.HasIndex(x => x.WarehouseId);

        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
