using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CampaignChannelConfig : IEntityTypeConfiguration<CampaignChannel>
{
    public void Configure(EntityTypeBuilder<CampaignChannel> builder)
    {
        // Table
        builder.ToTable("campaign_channels");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Channel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Channels)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.CampaignId, x.Channel }).IsUnique();
    }
}
