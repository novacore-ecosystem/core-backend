using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionStackingPolicyConfig : IEntityTypeConfiguration<PromotionStackingPolicy>
{
    public void Configure(EntityTypeBuilder<PromotionStackingPolicy> builder)
    {
        // Table
        builder.ToTable("promotion_stacking_policies");

        // Properties
        // Shared-PK 1:1 detail table - reuses the parent Promotion's Id via PromotionId (Rule 5).
        builder.HasKey(x => x.PromotionId);

        builder.Property(x => x.Mode).HasConversion<short>().IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithOne(x => x.StackingPolicy)
            .HasForeignKey<PromotionStackingPolicy>(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
