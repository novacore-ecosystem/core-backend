using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RecommendationScoreConfig : IEntityTypeConfiguration<RecommendationScore>
{
    public void Configure(EntityTypeBuilder<RecommendationScore> builder)
    {
        // Table
        builder.ToTable("recommendation_scores");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScoreType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ScoreValue).HasColumnType("numeric(9,4)").IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // ProductId is an external (Product service) reference - no local navigation.

        // Indexes
        builder.HasIndex(x => new { x.ProductId, x.ScoreType });
    }
}
