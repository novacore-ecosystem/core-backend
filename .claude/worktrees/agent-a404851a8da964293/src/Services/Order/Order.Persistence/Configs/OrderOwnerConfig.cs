using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderOwnerConfig : IEntityTypeConfiguration<OrderOwner>
{
    public void Configure(EntityTypeBuilder<OrderOwner> builder)
    {
        // Table
        builder.ToTable("order_owners");

        // Properties
        // Shared primary key (1:1 with Order) - no surrogate Id, exactly one owner row per order.
        builder.HasKey(x => x.OrderId);

        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.OwnerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.OwnerPhone)
            .HasConversion(x => x.Value, x => PhoneNumber.Create(x))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.OwnerPhoneSearch).HasMaxLength(20).IsRequired();
        builder.Property(x => x.OwnerPhoneReverse).HasMaxLength(20).IsRequired();
        builder.Property(x => x.OwnerEmail)
            .HasConversion(x => x.Value, x => Email.Create(x))
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // OrderOwner has no Order navigation property (one-directional, same reasoning as
        // OrderItem), so the "one" side is a shadow reference. HasForeignKey<OrderOwner>(x =>
        // x.OrderId) on a property that is also the primary key is what makes this a shared-PK
        // 1:1 association rather than a regular one-to-many.
        builder.HasOne<OrderEntity>()
            .WithOne(o => o.Owner)
            .HasForeignKey<OrderOwner>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // Scoped per customer (not globally unique) so two different customers may reuse the same
        // client-generated key. Partial index (WHERE clause) so orders with no key never collide.
        builder.HasIndex(x => new { x.OwnerId, x.IdempotencyKey })
            .IsUnique()
            .HasFilter("\"idempotency_key\" IS NOT NULL");

        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.OwnerName);
        builder.HasIndex(x => x.OwnerPhoneSearch);
        builder.HasIndex(x => x.OwnerPhoneReverse);
    }
}
