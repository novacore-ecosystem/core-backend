using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ApprovalAuditConfig : IEntityTypeConfiguration<ApprovalAudit>
{
    public void Configure(EntityTypeBuilder<ApprovalAudit> builder)
    {
        // Table
        builder.ToTable("approval_audits");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // WorkflowId stays FK-only, no local navigation - all four Audit entities are a single
        // deliberate uniform design (generic, not tied to one owning aggregate), so this one is
        // not special-cased even though its target type happens to be concretely known.

        // Indexes
        builder.HasIndex(x => x.WorkflowId);
    }
}
