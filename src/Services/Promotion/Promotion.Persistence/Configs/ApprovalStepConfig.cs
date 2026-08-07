using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ApprovalStepConfig : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        // Table
        builder.ToTable("approval_steps");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StepOrder).IsRequired();
        builder.Property(x => x.ApproverRole).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        // WorkflowId stays FK-only (no Workflow navigation) - preserves the one-directional
        // convention used by every other owned child in this project; ApprovalWorkflow.Steps is
        // the forward navigation. Shadow reference configured here since no local nav exists.
        builder.HasOne<ApprovalWorkflow>()
            .WithMany(x => x.Steps)
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Assignments/Decisions/Comments are all configured from the child entity's own config
        // (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.WorkflowId);
    }
}
