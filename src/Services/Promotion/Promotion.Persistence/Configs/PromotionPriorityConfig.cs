using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionPriorityConfig : IEntityTypeConfiguration<PromotionPriority>
{
    public void Configure(EntityTypeBuilder<PromotionPriority> builder)
    {
        // Table
        builder.ToTable("promotion_priorities");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PriorityType).HasConversion<short>().IsRequired();

        builder.Property(x => x.Value)
            .HasConversion(x => x.Value, x => PromotionPriorityValue.Create(x))
            .HasColumnName("value")
            .IsRequired();

        builder.Property(x => x.Note).HasMaxLength(500);

        builder.ConfigureAuditFields();

        // Relationships
        // Independently constructible, related to Promotion by PromotionId only - no reverse
        // collection on Promotion (not part of its owned navigation graph).
        builder.HasOne(x => x.Promotion)
            .WithMany()
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PromotionId);
    }
}
