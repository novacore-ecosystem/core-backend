using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionTargetConfig : IEntityTypeConfiguration<PromotionTarget>
{
    public void Configure(EntityTypeBuilder<PromotionTarget> builder)
    {
        // Table
        builder.ToTable("promotion_targets");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetType).HasConversion<short>().IsRequired();
        builder.Property(x => x.TargetKey).HasMaxLength(200).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(x => x.Targets)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PromotionId);
        builder.HasIndex(x => new { x.TargetType, x.TargetKey });
    }
}
