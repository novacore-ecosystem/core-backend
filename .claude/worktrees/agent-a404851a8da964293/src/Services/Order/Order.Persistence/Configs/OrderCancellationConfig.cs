using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderCancellationConfig : IEntityTypeConfiguration<OrderCancellation>
{
    public void Configure(EntityTypeBuilder<OrderCancellation> builder)
    {
        // Table
        builder.ToTable("order_cancellations");

        // Properties
        // Shared primary key (1:1 with Order) - no surrogate Id, same style as OrderOwner/
        // OrderPrice/OrderPayment. Optional (not every Order has one) - EF supports an optional
        // dependent on a shared-PK 1:1 without any extra config beyond the nullable navigation.
        builder.HasKey(x => x.OrderId);

        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CancelledByName).HasMaxLength(200);
        builder.Property(x => x.Comment).HasMaxLength(1000);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        builder.HasOne<OrderEntity>()
            .WithOne(o => o.Cancellation)
            .HasForeignKey<OrderCancellation>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
