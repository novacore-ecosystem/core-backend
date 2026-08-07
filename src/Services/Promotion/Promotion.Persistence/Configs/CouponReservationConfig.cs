using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CouponReservationConfig : IEntityTypeConfiguration<CouponReservation>
{
    public void Configure(EntityTypeBuilder<CouponReservation> builder)
    {
        // Table
        builder.ToTable("coupon_reservations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReservationToken).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CouponId);
        builder.HasIndex(x => x.ReservationToken).IsUnique();
    }
}
