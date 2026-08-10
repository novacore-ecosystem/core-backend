using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.Metadata;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.Order.Domain.Metadata;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderDiscountConfig : IEntityTypeConfiguration<OrderDiscount>
{
    public void Configure(EntityTypeBuilder<OrderDiscount> builder)
    {
        // Table
        builder.ToTable("order_discounts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.Target).HasConversion<int>().IsRequired();
        builder.Property(x => x.Source).HasConversion<int>().IsRequired();
        builder.Property(x => x.SourceId).HasMaxLength(100);
        builder.Property(x => x.SourceCode).HasMaxLength(100).IsRequired();

        builder.Property(x => x.AppliedAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x!.ToJson(),
                x => MetadataBase.FromJson<DiscountMetadata>(x))
            .HasColumnType("jsonb");

        // Rule is a 4-field composite VO (Method/Value/nullable MaximumDiscount/nullable
        // MinimumSpend, the latter two themselves wrapping Money) - owned rather than
        // HasConversion'd, since HasConversion only maps to a single column.
        builder.OwnsOne(x => x.Rule, rule =>
        {
            rule.Property(r => r.Method)
                .HasConversion<int>()
                .HasColumnName("rule_method")
                .IsRequired();

            rule.Property(r => r.Value)
                .HasColumnName("rule_value")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            rule.Property(r => r.MaximumDiscount)
                .HasConversion(x => x != null ? x.Value : (decimal?)null, x => x != null ? Money.Create(x.Value) : null)
                .HasColumnName("rule_maximum_discount")
                .HasColumnType("numeric(18,2)");

            rule.Property(r => r.MinimumSpend)
                .HasConversion(x => x != null ? x.Value : (decimal?)null, x => x != null ? Money.Create(x.Value) : null)
                .HasColumnName("rule_minimum_spend")
                .HasColumnType("numeric(18,2)");
        });
        builder.Navigation(x => x.Rule).IsRequired();

        // Relationships
        // Shadow reference to Order, same reasoning as OrderItem/OrderShipping - no
        // OrderDiscount->Order navigation exists on the domain side.
        builder.HasOne<OrderEntity>()
            .WithMany(o => o.Discounts)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItemId/OrderShippingId are optional pointers to which part of the order this
        // discount targets (see Target) - an Order-targeted discount (Target == Order) has
        // neither set. Explicit Restrict FKs (not Cascade) since OrderDiscount already cascades
        // from Order itself via OrderId above - a second cascade path here would be redundant.
        builder.HasOne<OrderItem>()
            .WithMany(i => i.Discounts)
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<OrderShipping>()
            .WithMany()
            .HasForeignKey(x => x.OrderShippingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.OrderItemId);
        builder.HasIndex(x => x.OrderShippingId);
    }
}
