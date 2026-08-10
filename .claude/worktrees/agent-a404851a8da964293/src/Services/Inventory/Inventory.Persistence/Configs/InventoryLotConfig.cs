using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventoryLotConfig : IEntityTypeConfiguration<InventoryLot>
{
    public void Configure(EntityTypeBuilder<InventoryLot> builder)
    {
        builder.ToTable("inventory_lots");

        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.InventoryId)
            .IsRequired();

        builder.Property(x => x.LotNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ManufactureDate)
            .IsRequired();

        builder.Property(x => x.ExpiredDate)
            .IsRequired();

        builder.Property(x => x.SupplierLotNumber)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.CountryOfOrigin)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(InventoryLotStatus.Active);

        builder.Property(x => x.Quantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x));

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => InventoryLotMetadata.FromJson<InventoryLotMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Inventory)
            .WithMany()
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.InventoryId);
        builder.HasIndex(x => x.LotNumber);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ExpiredDate);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
