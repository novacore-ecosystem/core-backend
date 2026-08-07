using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CouponApprovalConfig : IEntityTypeConfiguration<CouponApproval>
{
    public void Configure(EntityTypeBuilder<CouponApproval> builder)
    {
        // Table
        builder.ToTable("coupon_approvals");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.Comment).HasMaxLength(2000);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.Approvals)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CouponId);
    }
}
