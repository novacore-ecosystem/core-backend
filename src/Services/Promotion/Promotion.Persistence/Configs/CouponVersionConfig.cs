using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CouponVersionConfig : IEntityTypeConfiguration<CouponVersion>
{
    public void Configure(EntityTypeBuilder<CouponVersion> builder)
    {
        // Table
        builder.ToTable("coupon_versions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.Snapshot).HasColumnType("jsonb");

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CouponId);
    }
}
