using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PointAdjustmentConfig : IEntityTypeConfiguration<PointAdjustment>
{
    public void Configure(EntityTypeBuilder<PointAdjustment> builder)
    {
        // Table
        builder.ToTable("point_adjustments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Points).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(x => x.Adjustments)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.AccountId);
    }
}
