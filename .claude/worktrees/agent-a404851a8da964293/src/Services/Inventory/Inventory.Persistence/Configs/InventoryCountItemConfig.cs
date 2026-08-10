using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventoryCountItemConfig : IEntityTypeConfiguration<InventoryCountItem>
{
    public void Configure(EntityTypeBuilder<InventoryCountItem> builder)
    {
        builder.ToTable("inventory_count_items");
        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.InventoryCountId)
            .IsRequired();
        builder.Property(x => x.InventoryId)
            .IsRequired();
        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.ExpectedQuantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x))
            .IsRequired();

        builder.Property(x => x.ActualQuantity)
            .HasConversion(
                x => x != null ? x.Value : (int?)null,
                x => x.HasValue ? Quantity.Create(x.Value) : null)
            .IsRequired(false);

        builder.Property(x => x.DifferenceQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Note)
            .HasMaxLength(500)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        // Relationships
        builder.HasOne(x => x.InventoryCount)
            .WithMany(c => c.Items)
            .HasForeignKey(x => x.InventoryCountId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(x => x.Inventory)
            .WithMany()
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.InventoryCountId);
        builder.HasIndex(x => x.InventoryId);
    }
}
