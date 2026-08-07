using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RecommendationProductConfig : IEntityTypeConfiguration<RecommendationProduct>
{
    public void Configure(EntityTypeBuilder<RecommendationProduct> builder)
    {
        // Table
        builder.ToTable("recommendation_products");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score).HasColumnType("numeric(9,4)").IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.ProgramId, x.ProductId }).IsUnique();
    }
}
