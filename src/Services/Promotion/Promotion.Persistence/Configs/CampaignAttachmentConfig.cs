using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CampaignAttachmentConfig : IEntityTypeConfiguration<CampaignAttachment>
{
    public void Configure(EntityTypeBuilder<CampaignAttachment> builder)
    {
        // Table
        builder.ToTable("campaign_attachments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100);
        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CampaignId);
    }
}
