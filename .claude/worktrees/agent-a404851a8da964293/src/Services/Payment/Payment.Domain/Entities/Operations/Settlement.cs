namespace NovaCore.Payment.Domain.Entities.Operations;

/// <summary>Gateway settlement for a period - the gross/fee/net breakdown a gateway reports as having been paid out for a batch of Payments.</summary>
public sealed class Settlement : AggregateRoot<Guid>, IAuditable
{
    public Guid GatewayId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public Money GrossAmount { get; private set; } = default!;
    public Money FeeAmount { get; private set; } = default!;
    public Money NetAmount { get; private set; } = default!;
    public SettlementStatus Status { get; private set; }

    private Settlement() { }

    public static Settlement Create(Guid gatewayId, DateTime periodStart, DateTime periodEnd, Money grossAmount, Money feeAmount, Money netAmount)
    {
        if (periodEnd < periodStart)
            throw ExceptionFactory.InvalidRange("Settlement period end cannot be before period start.");

        return new Settlement
        {
            Id = Guid.CreateVersion7(),
            GatewayId = gatewayId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GrossAmount = grossAmount,
            FeeAmount = feeAmount,
            NetAmount = netAmount,
            Status = SettlementStatus.Pending,
        };
    }
}
