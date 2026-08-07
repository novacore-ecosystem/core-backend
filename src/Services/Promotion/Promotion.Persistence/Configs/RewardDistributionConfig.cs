using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RewardDistributionConfig : IEntityTypeConfiguration<RewardDistribution>
{
    public void Configure(EntityTypeBuilder<RewardDistribution> builder)
    {
        // Table
        builder.ToTable("reward_distributions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.Distributions)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cross-aggregate-group reference to an independent root - Restrict, not Cascade.
        builder.HasOne(x => x.DistributionJob)
            .WithMany()
            .HasForeignKey(x => x.DistributionJobId)
            .OnDelete(DeleteBehavior.Restrict);

        // Executions is configured from RewardExecutionConfig's side (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.ProgramId);
        builder.HasIndex(x => x.DistributionJobId);
    }
}
