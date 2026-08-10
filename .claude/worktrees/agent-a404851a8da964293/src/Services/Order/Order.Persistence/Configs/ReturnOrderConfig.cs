using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Order.Persistence.Configs;

public sealed class ReturnOrderConfig : IEntityTypeConfiguration<ReturnOrder>
{
    public void Configure(EntityTypeBuilder<ReturnOrder> builder)
    {
        // Table
        builder.ToTable("return_orders");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.Property(x => x.TotalRefundAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // Restrict, not Cascade - an Order with an active return should not be deletable out from
        // under it. Cross-aggregate reference (ReturnOrder is its own root, not a child of Order)
        // but still DB-enforced, same reasoning as OrderTag's FK to OrderTagDefinition.
        builder.HasOne<OrderEntity>()
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.ReturnOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Status);
    }
}
