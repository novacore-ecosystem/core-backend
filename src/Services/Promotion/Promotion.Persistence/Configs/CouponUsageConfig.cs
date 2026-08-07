using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CouponUsageConfig : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        // Table
        builder.ToTable("coupon_usages");

        // Properties
        builder.HasKey(x => x.Id);

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.Usages)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CouponId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.OrderId);
    }
}
