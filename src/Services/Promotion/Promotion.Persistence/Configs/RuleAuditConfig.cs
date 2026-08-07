using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RuleAuditConfig : IEntityTypeConfiguration<RuleAudit>
{
    public void Configure(EntityTypeBuilder<RuleAudit> builder)
    {
        // Table
        builder.ToTable("rule_audits");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // RuleId is a polymorphic reference (RecommendationRule, PointRule, PromotionRule, ...) -
        // no local navigation, matching every other Audit entity in this group.

        // Indexes
        builder.HasIndex(x => x.RuleId);
    }
}
