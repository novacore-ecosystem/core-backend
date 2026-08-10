using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderPriceConfig : IEntityTypeConfiguration<OrderPrice>
{
    public void Configure(EntityTypeBuilder<OrderPrice> builder)
    {
        // Table
        builder.ToTable("order_prices");

        // Properties
        // Shared primary key (1:1 with Order) - no surrogate Id, same style as OrderOwner.
        builder.HasKey(x => x.OrderId);

        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ExchangeRate).HasColumnType("numeric(18,6)");

        builder.Property(x => x.Subtotal)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.ItemDiscount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.PromotionDiscount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.CouponDiscount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.TaxAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.ServiceFee)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.PlatformFee)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.RoundingAdjustment)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.GrandTotal)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // OrderPrice has no Order navigation property (one-directional, same reasoning as
        // OrderOwner), so the "one" side is a shadow reference. HasForeignKey<OrderPrice>(x =>
        // x.OrderId) on a property that is also the primary key is what makes this a shared-PK
        // 1:1 association rather than a regular one-to-many.
        builder.HasOne<OrderEntity>()
            .WithOne(o => o.Price)
            .HasForeignKey<OrderPrice>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
