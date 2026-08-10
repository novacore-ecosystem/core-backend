using NovaCore.Inventory.Domain.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Inventory.Persistence.Configs;

public sealed class InventoryDocumentConfig : IEntityTypeConfiguration<InventoryDocument>
{
    public void Configure(EntityTypeBuilder<InventoryDocument> builder)
    {
        builder.ToTable("inventory_documents");
        builder.HasKey(x => x.Id);

        // Fields
        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(InventoryDocumentStatus.Draft);

        builder.Property(x => x.SourceWarehouseId);
        builder.Property(x => x.DestinationWarehouseId);

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.ApprovedAt);
        builder.Property(x => x.ApprovedBy);
        builder.Property(x => x.CompletedAt);

        builder
            .Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => InventoryDocumentMetadata.FromJson<InventoryDocumentMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.SourceWarehouse)
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(x => x.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.Document)
            .HasForeignKey(i => i.InventoryDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Number)
            .IsUnique();
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.SourceWarehouseId);
        builder.HasIndex(x => x.DestinationWarehouseId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
