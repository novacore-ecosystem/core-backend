using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RecommendationHistoryConfig : IEntityTypeConfiguration<RecommendationHistory>
{
    public void Configure(EntityTypeBuilder<RecommendationHistory> builder)
    {
        // Table
        builder.ToTable("recommendation_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProgramId);
    }
}
