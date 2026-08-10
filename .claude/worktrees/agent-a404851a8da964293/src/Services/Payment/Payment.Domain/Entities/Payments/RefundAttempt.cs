namespace NovaCore.Payment.Domain.Entities.Payments;

/// <summary>One gateway-facing attempt at processing a Refund - retry support, mirrors PaymentAttempt. Only Refund may construct one.</summary>
public sealed class RefundAttempt : BaseEntity<Guid>, IAuditable
{
    public Guid RefundId { get; private set; }
    public int AttemptNumber { get; private set; }
    public RefundAttemptStatus Status { get; private set; }
    public string? GatewayRefundId { get; private set; }
    public string? ResponsePayload { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureReason { get; private set; }

    private RefundAttempt() { }

    internal static RefundAttempt Create(Guid refundId, int attemptNumber)
    {
        if (attemptNumber < 1)
            throw ExceptionFactory.InvalidRange("Refund attempt number must be at least 1.");

        return new RefundAttempt
        {
            Id = Guid.CreateVersion7(),
            RefundId = refundId,
            AttemptNumber = attemptNumber,
            Status = RefundAttemptStatus.Initiated,
        };
    }
}
