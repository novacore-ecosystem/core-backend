using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CouponConfig : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        // Table
        builder.ToTable("coupons");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => EntityCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.Visibility).HasConversion<short>().IsRequired();
        builder.Property(x => x.CouponType).HasConversion<short>().IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MaxUsage);
        builder.Property(x => x.MaxUsagePerUser);
        builder.Property(x => x.CurrentUsage).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Promotion)
            .WithMany()
            .HasForeignKey(x => x.PromotionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Campaign)
            .WithMany()
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        // Batch relationship configured from CouponBatchConfig's side is not applicable (CouponBatch
        // has no Coupon nav) - kept here since this is the only side that declares it.
        builder.HasOne(x => x.Batch)
            .WithMany(x => x.Coupons)
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reservations/Usages/History/Versions/Codes/Approvals are all configured from the child
        // entity's own config (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Visibility, x.Status });
        builder.HasIndex(x => x.PromotionId);
        builder.HasIndex(x => x.CampaignId);
        builder.HasIndex(x => new { x.StartTime, x.EndTime });
    }
}
