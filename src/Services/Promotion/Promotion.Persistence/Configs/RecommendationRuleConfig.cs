using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RecommendationRuleConfig : IEntityTypeConfiguration<RecommendationRule>
{
    public void Configure(EntityTypeBuilder<RecommendationRule> builder)
    {
        // Table
        builder.ToTable("recommendation_rules");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RuleType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");
        builder.Property(x => x.Priority).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.Rules)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProgramId);
    }
}
