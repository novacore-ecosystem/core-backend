using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Order.Persistence.Configs;

public sealed class OrderTagConfig : IEntityTypeConfiguration<OrderTag>
{
    public void Configure(EntityTypeBuilder<OrderTag> builder)
    {
        // Table
        builder.ToTable("order_tags");

        // Properties
        // Pure mapping entity - the pairing itself is the identity, no surrogate Id.
        builder.HasKey(x => new { x.OrderId, x.TagId });

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        // Shadow references on both sides - no Order/OrderTagDefinition navigation exists on the
        // domain side, same reasoning as OrderItem/OrderTax/OrderDiscount.
        builder.HasOne<OrderEntity>()
            .WithMany(o => o.Tags)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade - a tag definition still assigned to orders should not be deletable.
        builder.HasOne<OrderTagDefinition>()
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        // Serves "find all orders with tag X" - the explicit filtering use case called out in
        // the OrderTag section of the Order Service architecture refactor plan.
        builder.HasIndex(x => x.TagId);
    }
}
