using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class TransportationTrackingConfig : IEntityTypeConfiguration<TransportationTracking>
{
    public void Configure(EntityTypeBuilder<TransportationTracking> builder)
    {
        // Table
        builder.ToTable("transportation_trackings");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransportationId).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.RecordedAt).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();

        builder.OwnsGeoCoordinate(x => x.Coordinate, "coordinate");

        // Append-only ping - never updated, so no concurrency token.
        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => new { x.TransportationId, x.RecordedAt });
    }
}
