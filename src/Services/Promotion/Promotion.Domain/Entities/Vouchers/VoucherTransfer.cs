namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>A record of a Voucher's ownership/balance being transferred between users - no ownership-change enforcement lives here.</summary>
public sealed class VoucherTransfer : BaseEntity<Guid>, IAuditable
{
    public Guid VoucherId { get; private set; }
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public DateTime TransferredAt { get; private set; }

    public Voucher Voucher { get; private set; } = default!;

    private VoucherTransfer() { }

    /// <summary>Only Voucher may construct a VoucherTransfer - see Voucher.AddTransfer.</summary>
    internal static VoucherTransfer Create(Guid voucherId, Guid fromUserId, Guid toUserId, Money amount)
    {
        return new VoucherTransfer
        {
            Id = Guid.CreateVersion7(),
            VoucherId = voucherId,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Amount = amount,
            TransferredAt = DateTime.UtcNow,
        };
    }
}
