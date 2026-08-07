namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>A record of a Voucher being frozen/unfrozen (e.g. fraud hold) - not a Voucher navigation collection (not listed in the requested Navigation set), so construction is public. No fraud/hold enforcement logic lives here.</summary>
public sealed class VoucherFreeze : BaseEntity<Guid>, IAuditable
{
    public Guid VoucherId { get; private set; }
    public string? Reason { get; private set; }
    public DateTime FrozenAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }

    public Voucher Voucher { get; private set; } = default!;

    private VoucherFreeze() { }

    public static VoucherFreeze Create(Guid voucherId, string? reason)
    {
        return new VoucherFreeze
        {
            Id = Guid.CreateVersion7(),
            VoucherId = voucherId,
            Reason = reason,
            FrozenAt = DateTime.UtcNow,
        };
    }

    public void Release()
    {
        ReleasedAt = DateTime.UtcNow;
    }
}
