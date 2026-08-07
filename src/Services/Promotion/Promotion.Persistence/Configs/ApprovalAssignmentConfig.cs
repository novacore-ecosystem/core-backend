using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ApprovalAssignmentConfig : IEntityTypeConfiguration<ApprovalAssignment>
{
    public void Configure(EntityTypeBuilder<ApprovalAssignment> builder)
    {
        // Table
        builder.ToTable("approval_assignments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Step)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.StepId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.StepId);
        builder.HasIndex(x => x.UserId);
    }
}
