using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RewardExecutionConfig : IEntityTypeConfiguration<RewardExecution>
{
    public void Configure(EntityTypeBuilder<RewardExecution> builder)
    {
        // Table
        builder.ToTable("reward_executions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExecutionKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Distribution)
            .WithMany(x => x.Executions)
            .HasForeignKey(x => x.DistributionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ExecutionKey).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}
