using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionSimulationConfig : IEntityTypeConfiguration<PromotionSimulation>
{
    public void Configure(EntityTypeBuilder<PromotionSimulation> builder)
    {
        // Table
        builder.ToTable("promotion_simulations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // Scenarios is configured from PromotionSimulationScenarioConfig's side (single source
        // per relationship).

        // Indexes
        builder.HasIndex(x => x.CreatedBy);
    }
}
