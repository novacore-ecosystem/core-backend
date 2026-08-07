using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CouponHistoryConfig : IEntityTypeConfiguration<CouponHistory>
{
    public void Configure(EntityTypeBuilder<CouponHistory> builder)
    {
        // Table
        builder.ToTable("coupon_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CouponId);
    }
}
