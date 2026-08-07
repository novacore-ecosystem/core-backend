using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CampaignBudgetConfig : IEntityTypeConfiguration<CampaignBudget>
{
    public void Configure(EntityTypeBuilder<CampaignBudget> builder)
    {
        // Table
        builder.ToTable("campaign_budgets");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AllocatedAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("allocated_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.SpentAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("spent_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasConversion(x => x.Value, x => Currency.Create(x))
            .HasMaxLength(3)
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Campaign)
            .WithMany()
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CampaignId);
    }
}
