using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionConstraintConfig : IEntityTypeConfiguration<PromotionConstraint>
{
    public void Configure(EntityTypeBuilder<PromotionConstraint> builder)
    {
        // Table
        builder.ToTable("promotion_constraints");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConstraintType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(x => x.Constraints)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PromotionId);
    }
}
