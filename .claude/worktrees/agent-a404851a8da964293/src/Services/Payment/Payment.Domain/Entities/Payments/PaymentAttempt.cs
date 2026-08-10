namespace NovaCore.Payment.Domain.Entities.Payments;

/// <summary>
/// One gateway-facing attempt at completing a Payment. A Payment may have multiple attempts
/// (e.g. Attempt #1 timeout -&gt; Attempt #2 success). Own table/PK, FK back to Payment - only
/// Payment may construct one.
/// </summary>
public sealed class PaymentAttempt : BaseEntity<Guid>, IAuditable
{
    public Guid PaymentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public PaymentAttemptStatus Status { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayOrderId { get; private set; }
    public string? RequestPayload { get; private set; }
    public string? ResponsePayload { get; private set; }
    public string? ResponseCode { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureReason { get; private set; }
    public string? RedirectUrl { get; private set; }
    public DateTime? AuthorizedAt { get; private set; }
    public DateTime? CapturedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private PaymentAttempt() { }

    internal static PaymentAttempt Create(Guid paymentId, int attemptNumber, DateTime? expiresAt = null)
    {
        if (attemptNumber < 1)
            throw ExceptionFactory.InvalidRange("Payment attempt number must be at least 1.");

        return new PaymentAttempt
        {
            Id = Guid.CreateVersion7(),
            PaymentId = paymentId,
            AttemptNumber = attemptNumber,
            Status = PaymentAttemptStatus.Initiated,
            ExpiresAt = expiresAt,
        };
    }
}
