namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>A temporary hold on a Coupon while checkout is in progress - no expiry sweeping/enforcement lives here.</summary>
public sealed class CouponReservation : BaseEntity<Guid>, IAuditable
{
    public Guid CouponId { get; private set; }
    public Guid UserId { get; private set; }
    public string ReservationToken { get; private set; } = string.Empty;
    public DateTime ReservedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public ReservationStatus Status { get; private set; } = ReservationStatus.Reserved;

    private CouponReservation() { }

    /// <summary>Only Coupon may construct a CouponReservation - see Coupon.AddReservation.</summary>
    internal static CouponReservation Create(Guid couponId, Guid userId, string reservationToken, DateTime? expiredAt)
    {
        if (string.IsNullOrWhiteSpace(reservationToken))
            throw ExceptionFactory.RequiredField("Reservation token cannot be empty.");

        return new CouponReservation
        {
            Id = Guid.CreateVersion7(),
            CouponId = couponId,
            UserId = userId,
            ReservationToken = reservationToken,
            ReservedAt = DateTime.UtcNow,
            ExpiredAt = expiredAt,
        };
    }

    public void UpdateStatus(ReservationStatus status)
    {
        Status = status;
    }
}
