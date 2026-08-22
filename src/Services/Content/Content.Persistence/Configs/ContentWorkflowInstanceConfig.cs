using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentWorkflowInstanceConfig : IEntityTypeConfiguration<ContentWorkflowInstance>
{
    public void Configure(EntityTypeBuilder<ContentWorkflowInstance> builder)
    {
        // Table
        builder.ToTable("content_workflow_instances");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentId).IsRequired();
        builder.Property(x => x.WorkflowDefinitionId).IsRequired();
        builder.Property(x => x.CurrentState).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StartedBy).IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.CompletedAt);

        // Relationships
        builder.HasOne(x => x.Content)
            .WithMany(c => c.WorkflowInstances)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.WorkflowDefinition)
            .WithMany()
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ContentId);
        builder.HasIndex(x => x.WorkflowDefinitionId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
