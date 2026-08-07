using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ExecutionAuditConfig : IEntityTypeConfiguration<ExecutionAudit>
{
    public void Configure(EntityTypeBuilder<ExecutionAudit> builder)
    {
        // Table
        builder.ToTable("execution_audits");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // ExecutionId is a polymorphic reference (RewardExecution, DistributionExecution, ...) -
        // no local navigation, matching every other Audit entity in this group.

        // Indexes
        builder.HasIndex(x => x.ExecutionId);
    }
}
