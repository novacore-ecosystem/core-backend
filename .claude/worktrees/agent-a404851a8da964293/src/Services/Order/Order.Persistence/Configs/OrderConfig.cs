using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.Order.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderConfig : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        // Table
        builder.ToTable("orders");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .HasConversion(x => x.Value, x => OrderNumber.Create(x))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status).HasConversion<int>();

        // ShippingFee/Subtotal/GrandTotal/CancellationReason are computed pass-throughs (Shipping.ShippingFee,
        // Price.Subtotal, Price.GrandTotal) - never persisted on Order itself. See OrderPriceConfig
        // for where Subtotal/GrandTotal actually live, and OrderTaxConfig for what used to be the
        // single owned Tax value object.
        builder.Ignore(x => x.ShippingFee);
        builder.Ignore(x => x.Subtotal);
        builder.Ignore(x => x.GrandTotal);
        builder.Ignore(x => x.CancellationReason);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Guards concurrent writers of the same Order row (CreateOrderSaga's ConfirmOrderStep vs
        // a manual CancelOrder request racing each other) - see EfUnitOfWork.ExecuteTransactionAsync,
        // which translates the resulting DbUpdateConcurrencyException into ConflictException.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();

        // Relationships
        // OrderItem has no independent identity outside its Order but is a normal related entity
        // (own table, own PK, FK back to Order) - see OrderItemConfig, which owns the other side
        // of this relationship. CustomerId/contact/shipping/IdempotencyKey now live on the 1:1
        // OrderOwner - see OrderOwnerConfig. Neither is auto-loaded with the Order the way an
        // owned collection/type was; every read path that needs them now Includes explicitly.
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Taxes relationship is configured from the child side - see OrderTaxConfig, same
        // pattern OrderDiscountConfig already uses for Order.Discounts.

        // Indexes
        // CreatedAt is the sort key for both admin search (OrderCriteriaDefinition) and customer
        // history (OrderHistoryCriteriaDefinition) - kept standalone for sort-only/unfiltered
        // listings, and leading a composite with Status since the two are commonly filtered+sorted
        // together (the composite also serves Status-only filters via its leftmost column, so the
        // old lone Status index was removed as redundant).
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
