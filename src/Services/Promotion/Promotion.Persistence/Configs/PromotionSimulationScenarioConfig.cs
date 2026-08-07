using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionSimulationScenarioConfig : IEntityTypeConfiguration<PromotionSimulationScenario>
{
    public void Configure(EntityTypeBuilder<PromotionSimulationScenario> builder)
    {
        // Table
        builder.ToTable("promotion_simulation_scenarios");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Input).HasColumnType("jsonb");

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Simulation)
            .WithMany(x => x.Scenarios)
            .HasForeignKey(x => x.SimulationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Results is configured from PromotionSimulationResultConfig's side (single source per
        // relationship).

        // Indexes
        builder.HasIndex(x => x.SimulationId);
    }
}
