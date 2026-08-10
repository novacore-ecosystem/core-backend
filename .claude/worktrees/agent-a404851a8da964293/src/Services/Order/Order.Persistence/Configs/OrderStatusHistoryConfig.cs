using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderStatusHistoryConfig : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        // Table
        builder.ToTable("order_status_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.EventType).HasConversion<int>().IsRequired();
        builder.Property(x => x.PreviousStatus).HasConversion<int?>();
        builder.Property(x => x.CurrentStatus).HasConversion<int?>();
        builder.Property(x => x.ChangedByName).HasMaxLength(200);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.Comment).HasMaxLength(1000);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // Shadow reference to Order, same reasoning as OrderItem/OrderTax/OrderDiscount - no
        // OrderStatusHistory->Order navigation exists on the domain side. This is pure history,
        // so unlike OrderDiscount's Restrict FKs, deleting an Order cascades to its history too.
        builder.HasOne<OrderEntity>()
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // (OrderId, ChangedAt) serves a per-order timeline read (filter by OrderId, sort by
        // ChangedAt) in one index, same reasoning as OrderConfig's (Status, CreatedAt) composite.
        builder.HasIndex(x => new { x.OrderId, x.ChangedAt });
    }
}
