using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ApprovalDecisionConfig : IEntityTypeConfiguration<ApprovalDecision>
{
    public void Configure(EntityTypeBuilder<ApprovalDecision> builder)
    {
        // Table
        builder.ToTable("approval_decisions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Decision).HasConversion<short>().IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Step)
            .WithMany(x => x.Decisions)
            .HasForeignKey(x => x.StepId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.StepId);
    }
}
