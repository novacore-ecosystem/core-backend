namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>A record of a Voucher being issued/distributed to a user - no distribution-channel logic lives here.</summary>
public sealed class VoucherIssue : BaseEntity<Guid>, IAuditable
{
    public Guid VoucherId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? DistributionId { get; private set; }
    public DateTime IssuedAt { get; private set; }

    public Voucher Voucher { get; private set; } = default!;

    private VoucherIssue() { }

    /// <summary>Only Voucher may construct a VoucherIssue - see Voucher.AddIssue.</summary>
    internal static VoucherIssue Create(Guid voucherId, Guid userId, Guid? distributionId)
    {
        return new VoucherIssue
        {
            Id = Guid.CreateVersion7(),
            VoucherId = voucherId,
            UserId = userId,
            DistributionId = distributionId,
            IssuedAt = DateTime.UtcNow,
        };
    }
}
