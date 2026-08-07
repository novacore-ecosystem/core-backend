namespace NovaCore.Promotion.Domain.Entities.Vouchers;

/// <summary>An append-only audit trail row for a Voucher - CreatedAt is inherited from BaseEntity, not redeclared.</summary>
public sealed class VoucherHistory : BaseEntity<Guid>, IAuditable
{
    public Guid VoucherId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    public Voucher Voucher { get; private set; } = default!;

    private VoucherHistory() { }

    /// <summary>Only Voucher may construct a VoucherHistory - see Voucher.RecordHistory.</summary>
    internal static VoucherHistory Create(Guid voucherId, string action, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("History action cannot be empty.");

        return new VoucherHistory
        {
            Id = Guid.CreateVersion7(),
            VoucherId = voucherId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
