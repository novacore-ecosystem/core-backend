using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CampaignScheduleConfig : IEntityTypeConfiguration<CampaignSchedule>
{
    public void Configure(EntityTypeBuilder<CampaignSchedule> builder)
    {
        // Table
        builder.ToTable("campaign_schedules");

        // Properties
        builder.HasKey(x => x.Id);

        builder.OwnsPeriod(x => x.Period, "period");

        builder.Property(x => x.Label).HasMaxLength(200);
        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CampaignId);
    }
}
