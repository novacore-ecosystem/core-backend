using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class TransportationConfig : IEntityTypeConfiguration<Transportation>
{
    public void Configure(EntityTypeBuilder<Transportation> builder)
    {
        // Table
        builder.ToTable("transportations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransportationNumber)
            .HasConversion(x => x.Value, x => TransportationNumber.Create(x))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ShipmentId).IsRequired();
        builder.Property(x => x.ProviderId).IsRequired();
        builder.Property(x => x.AttemptNo).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.DistanceKm).HasColumnType("numeric(10,3)");
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);
        builder.Property(x => x.TotalCost)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        // Assignment/Proof are shared-PK 1:1 children (see their own configs, which own that side
        // of the association). Trackings/Costs are ordinary FK collections.
        builder.HasMany(x => x.Trackings)
            .WithOne()
            .HasForeignKey(t => t.TransportationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Costs)
            .WithOne()
            .HasForeignKey(c => c.TransportationId)
            .OnDelete(DeleteBehavior.Cascade);

        // ShipmentId/ProviderId/CostRuleId are indexed references, not configured FKs: Shipment
        // and Transportation are separate aggregate roots, and this codebase does not wire FK
        // navigations across aggregate boundaries (see Order's Order/ReturnOrder split).

        // Indexes
        builder.HasIndex(x => x.TransportationNumber).IsUnique();
        builder.HasIndex(x => x.ShipmentId);
        builder.HasIndex(x => new { x.ShipmentId, x.AttemptNo }).IsUnique();
        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => x.CostRuleId);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
