using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class ApprovalCommentConfig : IEntityTypeConfiguration<ApprovalComment>
{
    public void Configure(EntityTypeBuilder<ApprovalComment> builder)
    {
        // Table
        builder.ToTable("approval_comments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment).HasMaxLength(2000).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Step)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.StepId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.StepId);
    }
}
