using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventoryDocumentItemConfig : IEntityTypeConfiguration<InventoryDocumentItem>
{
    public void Configure(EntityTypeBuilder<InventoryDocumentItem> builder)
    {
        builder.ToTable("inventory_document_items");
        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.InventoryDocumentId)
            .IsRequired();
        builder.Property(x => x.ProductId)
            .IsRequired();
        builder.Property(x => x.VariantId)
            .IsRequired();
        builder.Property(x => x.InventoryId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasConversion(
                x => x.Value,
                x => Quantity.Create(x))
            .IsRequired();

        builder.Property(x => x.UnitOfMeasure)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.InventoryLotId);
        builder.Property(x => x.InventorySerialId);

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => InventoryDocumentItemMetadata.FromJson<InventoryDocumentItemMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Document)
            .WithMany(d => d.Items)
            .HasForeignKey(x => x.InventoryDocumentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(x => x.Inventory)
            .WithMany()
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.InventoryDocumentId);
        builder.HasIndex(x => x.InventoryId);
        builder.HasIndex(x => x.VariantId);
    }
}
