using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentWorkflowTransitionConfig : IEntityTypeConfiguration<ContentWorkflowTransition>
{
    public void Configure(EntityTypeBuilder<ContentWorkflowTransition> builder)
    {
        // Table
        builder.ToTable("content_workflow_transitions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkflowDefinitionId).IsRequired();
        builder.Property(x => x.FromStateId).IsRequired();
        builder.Property(x => x.ToStateId).IsRequired();

        builder.Property(x => x.Key)
            .HasConversion(x => x.Value, x => ContentKey.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.WorkflowDefinition)
            .WithMany(d => d.Transitions)
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // FromState/ToState use Restrict, not Cascade - States are already cascade-deleted via
        // their own WorkflowDefinitionId FK when the whole definition goes; Restrict here just
        // protects a single targeted State deletion while transitions still reference it.
        builder.HasOne(x => x.FromState)
            .WithMany(s => s.OutgoingTransitions)
            .HasForeignKey(x => x.FromStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToState)
            .WithMany(s => s.IncomingTransitions)
            .HasForeignKey(x => x.ToStateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.WorkflowDefinitionId);
        builder.HasIndex(x => new { x.FromStateId, x.ToStateId }).IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
