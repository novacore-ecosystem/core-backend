namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>A temporary hold on part of a Voucher's balance while checkout is in progress - no expiry sweeping/enforcement lives here.</summary>
public sealed class VoucherReservation : BaseEntity<Guid>, IAuditable
{
    public Guid VoucherId { get; private set; }
    public Guid? OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public Money ReservedAmount { get; private set; } = default!;
    public DateTime ReservedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }

    public Voucher Voucher { get; private set; } = default!;

    private VoucherReservation() { }

    /// <summary>Only Voucher may construct a VoucherReservation - see Voucher.AddReservation.</summary>
    internal static VoucherReservation Create(Guid voucherId, Guid? orderId, Guid userId, Money reservedAmount, DateTime? expiredAt)
    {
        return new VoucherReservation
        {
            Id = Guid.CreateVersion7(),
            VoucherId = voucherId,
            OrderId = orderId,
            UserId = userId,
            ReservedAmount = reservedAmount,
            ReservedAt = DateTime.UtcNow,
            ExpiredAt = expiredAt,
        };
    }
}
