using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionUsageLimitConfig : IEntityTypeConfiguration<PromotionUsageLimit>
{
    public void Configure(EntityTypeBuilder<PromotionUsageLimit> builder)
    {
        // Table
        builder.ToTable("promotion_usage_limits");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Scope).HasConversion<short>().IsRequired();
        builder.Property(x => x.MaxUsage).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(x => x.UsageLimits)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.PromotionId, x.Scope }).IsUnique();
    }
}
