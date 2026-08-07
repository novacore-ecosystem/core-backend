using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class GiftItemConfig : IEntityTypeConfiguration<GiftItem>
{
    public void Configure(EntityTypeBuilder<GiftItem> builder)
    {
        // Table
        builder.ToTable("gift_items");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("quantity")
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inventories/Reservations/Usages are all configured from the child entity's own config
        // (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.ProgramId);
        builder.HasIndex(x => x.ProductId);
    }
}
