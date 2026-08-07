using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RecommendationProgramConfig : IEntityTypeConfiguration<RecommendationProgram>
{
    public void Configure(EntityTypeBuilder<RecommendationProgram> builder)
    {
        // Table
        builder.ToTable("recommendation_programs");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => EntityCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.RecommendationType).HasConversion<short>().IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.Priority).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        // Rules/Products/History are all configured from the child entity's own config
        // (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
