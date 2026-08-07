using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionSimulationResultConfig : IEntityTypeConfiguration<PromotionSimulationResult>
{
    public void Configure(EntityTypeBuilder<PromotionSimulationResult> builder)
    {
        // Table
        builder.ToTable("promotion_simulation_results");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Output).HasColumnType("jsonb");
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Scenario)
            .WithMany(x => x.Results)
            .HasForeignKey(x => x.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ScenarioId);
    }
}
