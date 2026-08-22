using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentWorkflowDefinitionConfig : IEntityTypeConfiguration<ContentWorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<ContentWorkflowDefinition> builder)
    {
        // Table
        builder.ToTable("content_workflow_definitions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasConversion(x => x.Value, x => ContentKey.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        // Relationships
        // States/Transitions are configured from their own configs (single source).

        // Indexes
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
