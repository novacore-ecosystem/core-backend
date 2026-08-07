using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CampaignApprovalConfig : IEntityTypeConfiguration<CampaignApproval>
{
    public void Configure(EntityTypeBuilder<CampaignApproval> builder)
    {
        // Table
        builder.ToTable("campaign_approvals");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment).HasMaxLength(2000);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Approvals)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CampaignId);
    }
}
