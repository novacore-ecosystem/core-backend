using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderPaymentConfig : IEntityTypeConfiguration<OrderPayment>
{
    public void Configure(EntityTypeBuilder<OrderPayment> builder)
    {
        // Table
        builder.ToTable("order_payments");

        // Properties
        // Shared primary key (1:1 with Order) - no surrogate Id, same style as OrderOwner/OrderPrice.
        builder.HasKey(x => x.OrderId);

        builder.Property(x => x.PaymentStatus).HasConversion<int>().IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        builder.Property(x => x.PaidAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // OrderPayment has no Order navigation property (one-directional, same reasoning as
        // OrderOwner/OrderPrice), so the "one" side is a shadow reference.
        builder.HasOne<OrderEntity>()
            .WithOne(o => o.Payment)
            .HasForeignKey<OrderPayment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PaymentId);
    }
}
