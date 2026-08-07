using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionValidationPolicyConfig : IEntityTypeConfiguration<PromotionValidationPolicy>
{
    public void Configure(EntityTypeBuilder<PromotionValidationPolicy> builder)
    {
        // Table
        builder.ToTable("promotion_validation_policies");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RuleType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.Property(x => x.Priority).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // Results is configured from PromotionValidationResultConfig's side (single source per
        // relationship).

        // Indexes
        builder.HasIndex(x => x.RuleType);
    }
}
