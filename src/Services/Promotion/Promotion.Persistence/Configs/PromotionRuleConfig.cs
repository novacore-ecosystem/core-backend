using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionRuleConfig : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        // Table
        builder.ToTable("promotion_rules");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(x => x.Rules)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RuleGroup)
            .WithMany(x => x.Rules)
            .HasForeignKey(x => x.RuleGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Conditions)
            .WithOne(x => x.Rule)
            .HasForeignKey(x => x.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PromotionId);
        builder.HasIndex(x => x.RuleGroupId);
    }
}
