namespace NovaCore.Payment.Domain.Entities.Operations;

/// <summary>A fee (gateway, platform, or tax) charged against either a Payment or a Settlement - exactly one of PaymentId/SettlementId is set.</summary>
public sealed class PaymentFee : BaseEntity<Guid>, IAuditable
{
    public Guid? PaymentId { get; private set; }
    public Guid? SettlementId { get; private set; }
    public FeeType FeeType { get; private set; }
    public Money Amount { get; private set; } = default!;
    public string? Description { get; private set; }

    private PaymentFee() { }

    public static PaymentFee ForPayment(Guid paymentId, FeeType feeType, Money amount, string? description = null)
        => Create(paymentId, null, feeType, amount, description);

    public static PaymentFee ForSettlement(Guid settlementId, FeeType feeType, Money amount, string? description = null)
        => Create(null, settlementId, feeType, amount, description);

    private static PaymentFee Create(Guid? paymentId, Guid? settlementId, FeeType feeType, Money amount, string? description)
    {
        if (paymentId is null && settlementId is null)
            throw ExceptionFactory.RequiredField("A payment fee must be linked to either a Payment or a Settlement.");

        return new PaymentFee
        {
            Id = Guid.CreateVersion7(),
            PaymentId = paymentId,
            SettlementId = settlementId,
            FeeType = feeType,
            Amount = amount,
            Description = description,
        };
    }
}
