using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class TransportationCostConfig : IEntityTypeConfiguration<TransportationCost>
{
    public void Configure(EntityTypeBuilder<TransportationCost> builder)
    {
        // Table
        builder.ToTable("transportation_costs");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransportationId).IsRequired();
        builder.Property(x => x.Category).HasConversion<short>().IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IncurredAt).IsRequired();
        builder.Property(x => x.Amount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.TransportationId);
        builder.HasIndex(x => new { x.TransportationId, x.Category });
    }
}
