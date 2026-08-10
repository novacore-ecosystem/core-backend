using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class ReturnItemConfig : IEntityTypeConfiguration<ReturnItem>
{
    public void Configure(EntityTypeBuilder<ReturnItem> builder)
    {
        // Table
        builder.ToTable("return_items");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReturnOrderId).IsRequired();
        builder.Property(x => x.OrderItemId).IsRequired();
        builder.Property(x => x.ReasonId).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000);

        builder.Property(x => x.Quantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .IsRequired();

        builder.Property(x => x.RefundAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // ReturnOrderId relation is configured from the parent side - see ReturnOrderConfig, same
        // pattern OrderConfig uses for Order.Items.
        // Restrict, not Cascade - an OrderItem referenced by a return should not be deletable
        // (OrderItem is effectively immutable/append-only in practice, but the constraint still
        // documents the dependency).
        builder.HasOne<OrderItem>()
            .WithMany()
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ReturnReason>()
            .WithMany()
            .HasForeignKey(x => x.ReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ReturnOrderId);
        builder.HasIndex(x => x.OrderItemId);
        builder.HasIndex(x => x.ReasonId);
    }
}
