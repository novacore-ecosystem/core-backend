using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventoryCountConfig : IEntityTypeConfiguration<InventoryCount>
{
    public void Configure(EntityTypeBuilder<InventoryCount> builder)
    {
        builder.ToTable("inventory_counts");
        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.WarehouseId).IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(InventoryCountStatus.Draft);

        builder.Property(x => x.CountDate).IsRequired();
        builder.Property(x => x.ApprovedBy);
        builder.Property(x => x.ApprovedAt);

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        // Relationships
        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne(i => i.InventoryCount)
            .HasForeignKey(i => i.InventoryCountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CountDate);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
