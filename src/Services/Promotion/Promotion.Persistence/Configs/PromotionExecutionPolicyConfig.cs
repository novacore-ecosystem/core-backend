using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionExecutionPolicyConfig : IEntityTypeConfiguration<PromotionExecutionPolicy>
{
    public void Configure(EntityTypeBuilder<PromotionExecutionPolicy> builder)
    {
        // Table
        builder.ToTable("promotion_execution_policies");

        // Properties
        // Shared-PK 1:1 detail table - reuses the parent Promotion's Id via PromotionId (Rule 5).
        builder.HasKey(x => x.PromotionId);

        builder.Property(x => x.Mode).HasConversion<short>().IsRequired();
        builder.Property(x => x.MaxExecutionsPerOrder);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithOne(x => x.ExecutionPolicy)
            .HasForeignKey<PromotionExecutionPolicy>(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
