using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderShippingConfig : IEntityTypeConfiguration<OrderShipping>
{
    public void Configure(EntityTypeBuilder<OrderShipping> builder)
    {
        // Table
        builder.ToTable("order_shippings");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();

        builder.Property(x => x.ReceiverName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ReceiverPhone)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500).IsRequired();

        builder.Property(x => x.ShippingMethod).HasConversion<int>().IsRequired();
        builder.Property(x => x.Carrier).HasMaxLength(100);
        builder.Property(x => x.TrackingNumber).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.Property(x => x.ShippingFee)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // Order.Shipping has no reverse OrderShipping->Order navigation, same reasoning as
        // OrderItem's shadow reference - a required 1:1 keyed by a unique OrderId FK, not a
        // shared primary key the way OrderOwner is.
        builder.HasOne<OrderEntity>()
            .WithOne(o => o.Shipping)
            .HasForeignKey<OrderShipping>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => x.TrackingNumber);
    }
}
