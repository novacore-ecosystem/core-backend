using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionRuleGroupConfig : IEntityTypeConfiguration<PromotionRuleGroup>
{
    public void Configure(EntityTypeBuilder<PromotionRuleGroup> builder)
    {
        // Table
        builder.ToTable("promotion_rule_groups");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LogicOperator).HasConversion<short>().IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // Independently constructible, related to Promotion by PromotionId only - no reverse
        // collection on Promotion (Promotion does not own PromotionRuleGroup construction).
        builder.HasOne(x => x.Promotion)
            .WithMany()
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Rules is a query-only reverse of PromotionRule.RuleGroupId - Promotion owns PromotionRule
        // construction, not PromotionRuleGroup, so this relationship is configured from
        // PromotionRuleConfig's side.

        // Indexes
        builder.HasIndex(x => x.PromotionId);
    }
}
