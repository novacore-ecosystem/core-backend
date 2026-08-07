namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>A single redemption record for a Voucher - no balance-debit calculation lives here, see Voucher.AddRedemption/UpdateBalance.</summary>
public sealed class VoucherRedemption : BaseEntity<Guid>, IAuditable
{
    public Guid VoucherId { get; private set; }
    public Guid? OrderId { get; private set; }
    public Money RedeemedAmount { get; private set; } = default!;
    public DateTime RedeemedAt { get; private set; }

    public Voucher Voucher { get; private set; } = default!;

    private VoucherRedemption() { }

    /// <summary>Only Voucher may construct a VoucherRedemption - see Voucher.AddRedemption.</summary>
    internal static VoucherRedemption Create(Guid voucherId, Guid? orderId, Money redeemedAmount)
    {
        return new VoucherRedemption
        {
            Id = Guid.CreateVersion7(),
            VoucherId = voucherId,
            OrderId = orderId,
            RedeemedAmount = redeemedAmount,
            RedeemedAt = DateTime.UtcNow,
        };
    }
}
