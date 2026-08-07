using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionAuditConfig : IEntityTypeConfiguration<PromotionAudit>
{
    public void Configure(EntityTypeBuilder<PromotionAudit> builder)
    {
        // Table
        builder.ToTable("promotion_audits");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // AggregateId is a polymorphic reference (could target any aggregate type) - no local
        // navigation, matching every other Audit entity in this group.

        // Indexes
        builder.HasIndex(x => x.AggregateId);
    }
}
