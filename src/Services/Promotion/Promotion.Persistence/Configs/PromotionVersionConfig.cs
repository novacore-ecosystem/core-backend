using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionVersionConfig : IEntityTypeConfiguration<PromotionVersion>
{
    public void Configure(EntityTypeBuilder<PromotionVersion> builder)
    {
        // Table
        builder.ToTable("promotion_versions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VersionNumber).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(2000);

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PromotionId);
    }
}
