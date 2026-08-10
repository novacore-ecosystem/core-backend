using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class ShipmentEventConfig : IEntityTypeConfiguration<ShipmentEvent>
{
    public void Configure(EntityTypeBuilder<ShipmentEvent> builder)
    {
        // Table
        builder.ToTable("shipment_events");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShipmentId).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();

        // Append-only timeline row - no concurrency token, it is never updated (same reasoning as
        // Payment's PaymentEventLog).
        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => new { x.ShipmentId, x.OccurredAt });
    }
}
