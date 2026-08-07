using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CouponBatchConfig : IEntityTypeConfiguration<CouponBatch>
{
    public void Configure(EntityTypeBuilder<CouponBatch> builder)
    {
        // Table
        builder.ToTable("coupon_batches");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(200);
        builder.Property(x => x.TotalCount).IsRequired();
        builder.Property(x => x.ActivatedCount).IsRequired();
        builder.Property(x => x.UsedCount).IsRequired();
        builder.Property(x => x.FailedCount).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships configured from CouponConfig/CouponCodeConfig (Coupons/Codes collections).
    }
}
