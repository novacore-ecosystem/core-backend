using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionConditionConfig : IEntityTypeConfiguration<PromotionCondition>
{
    public void Configure(EntityTypeBuilder<PromotionCondition> builder)
    {
        // Table
        builder.ToTable("promotion_conditions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Field).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Operator).HasConversion<short>().IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Rule)
            .WithMany(x => x.Conditions)
            .HasForeignKey(x => x.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.RuleId);
    }
}
