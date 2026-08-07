using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherIssueConfig : IEntityTypeConfiguration<VoucherIssue>
{
    public void Configure(EntityTypeBuilder<VoucherIssue> builder)
    {
        // Table
        builder.ToTable("voucher_issues");

        // Properties
        builder.HasKey(x => x.Id);

        builder.ConfigureAuditFields();

        // Relationships
        // DistributionId's target entity was never confirmed (Phase 2.6 audit note) - stays FK-only.
        builder.HasOne(x => x.Voucher)
            .WithMany(x => x.Issues)
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.VoucherId);
        builder.HasIndex(x => x.UserId);
    }
}
