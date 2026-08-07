using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CampaignAudienceConfig : IEntityTypeConfiguration<CampaignAudience>
{
    public void Configure(EntityTypeBuilder<CampaignAudience> builder)
    {
        // Table
        builder.ToTable("campaign_audiences");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Audiences)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CampaignId);
    }
}
