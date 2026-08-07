using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionValidationResultConfig : IEntityTypeConfiguration<PromotionValidationResult>
{
    public void Configure(EntityTypeBuilder<PromotionValidationResult> builder)
    {
        // Table
        builder.ToTable("promotion_validation_results");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000);

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Policy)
            .WithMany(x => x.Results)
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PolicyId);
    }
}
