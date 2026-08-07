using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class GiftInventoryConfig : IEntityTypeConfiguration<GiftInventory>
{
    public void Configure(EntityTypeBuilder<GiftInventory> builder)
    {
        // Table
        builder.ToTable("gift_inventories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AvailableQuantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("available_quantity")
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.GiftItem)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.GiftItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.GiftItemId, x.WarehouseId }).IsUnique();
    }
}
