using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CampaignTagConfig : IEntityTypeConfiguration<CampaignTag>
{
    public void Configure(EntityTypeBuilder<CampaignTag> builder)
    {
        // Table
        builder.ToTable("campaign_tags");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Label).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.CampaignId, x.Label }).IsUnique();
    }
}
