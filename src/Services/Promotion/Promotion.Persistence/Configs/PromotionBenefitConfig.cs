using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionBenefitConfig : IEntityTypeConfiguration<PromotionBenefit>
{
    public void Configure(EntityTypeBuilder<PromotionBenefit> builder)
    {
        // Table
        builder.ToTable("promotion_benefits");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BenefitType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Value).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(x => x.Benefits)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PromotionId);
    }
}
