namespace NovaCore.Payment.Domain.Entities.Operations;

/// <summary>Append-only business event history (PaymentCreated, PaymentAuthorized, PaymentCaptured, RefundCompleted, PaymentExpired, ...). Write-once - no update methods.</summary>
public sealed class PaymentEventLog : BaseEntity<Guid>
{
    public ReferenceType ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public PaymentEventType EventType { get; private set; }
    public string? Details { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private PaymentEventLog() { }

    public static PaymentEventLog Create(ReferenceType referenceType, Guid referenceId, PaymentEventType eventType, Guid? paymentId = null, string? details = null)
    {
        return new PaymentEventLog
        {
            Id = Guid.CreateVersion7(),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            PaymentId = paymentId,
            EventType = eventType,
            Details = details,
            OccurredAt = DateTime.UtcNow,
        };
    }
}
