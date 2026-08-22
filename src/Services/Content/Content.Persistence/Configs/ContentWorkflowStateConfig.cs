using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentWorkflowStateConfig : IEntityTypeConfiguration<ContentWorkflowState>
{
    public void Configure(EntityTypeBuilder<ContentWorkflowState> builder)
    {
        // Table
        builder.ToTable("content_workflow_states");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkflowDefinitionId).IsRequired();

        builder.Property(x => x.Key)
            .HasConversion(x => x.Value, x => ContentKey.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsInitial).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsFinal).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.WorkflowDefinition)
            .WithMany(d => d.States)
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // OutgoingTransitions/IncomingTransitions are configured from
        // ContentWorkflowTransitionConfig (single source per relationship).

        // Indexes
        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.Key }).IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
