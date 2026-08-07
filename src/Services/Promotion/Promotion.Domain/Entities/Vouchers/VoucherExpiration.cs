namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>A record of (part of) a Voucher's balance expiring - not a Voucher navigation collection (not listed in the requested Navigation set), so construction is public. No expiry sweeping logic lives here.</summary>
public sealed class VoucherExpiration : BaseEntity<Guid>, IAuditable
{
    public Guid VoucherId { get; private set; }
    public Money ExpiredAmount { get; private set; } = default!;
    public DateTime ExpiredAt { get; private set; }

    public Voucher Voucher { get; private set; } = default!;

    private VoucherExpiration() { }

    public static VoucherExpiration Create(Guid voucherId, Money expiredAmount)
    {
        return new VoucherExpiration
        {
            Id = Guid.CreateVersion7(),
            VoucherId = voucherId,
            ExpiredAmount = expiredAmount,
            ExpiredAt = DateTime.UtcNow,
        };
    }
}
