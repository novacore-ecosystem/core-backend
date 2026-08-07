using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ApprovalWorkflowConfig : IEntityTypeConfiguration<ApprovalWorkflow>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        // Table
        builder.ToTable("approval_workflows");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkflowType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        // Steps relationship is configured from ApprovalStepConfig's side (shadow WorkflowId nav).

        builder.HasMany(x => x.History)
            .WithOne(x => x.Workflow)
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Status);
    }
}
