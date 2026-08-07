using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionExclusionConfig : IEntityTypeConfiguration<PromotionExclusion>
{
    public void Configure(EntityTypeBuilder<PromotionExclusion> builder)
    {
        // Table
        builder.ToTable("promotion_exclusions");

        // Properties
        // Pure mapping entity - the pairing itself is the identity, no surrogate Id.
        builder.HasKey(x => new { x.PromotionId, x.ExcludedPromotionId });

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany()
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade - avoids a second cascade path back through the same Promotion
        // table (both FKs target Promotion.Id).
        builder.HasOne(x => x.ExcludedPromotion)
            .WithMany()
            .HasForeignKey(x => x.ExcludedPromotionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ExcludedPromotionId);
    }
}
