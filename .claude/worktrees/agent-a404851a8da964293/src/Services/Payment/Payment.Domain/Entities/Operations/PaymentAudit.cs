namespace NovaCore.Payment.Domain.Entities.Operations;

/// <summary>
/// Append-only business audit trail for a Payment (who did what, when). Explicitly not a
/// replacement for the platform-wide AuditService - this only covers payment-domain actions
/// PaymentService itself needs to answer questions about, independent of AuditService's own
/// cross-cutting change tracking.
/// </summary>
public sealed class PaymentAudit : BaseEntity<Guid>
{
    public Guid PaymentId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? ActorReferenceId { get; private set; }
    public string? Snapshot { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private PaymentAudit() { }

    public static PaymentAudit Create(Guid paymentId, string action, Guid? actorReferenceId = null, string? snapshot = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("Payment audit action cannot be empty.");

        return new PaymentAudit
        {
            Id = Guid.CreateVersion7(),
            PaymentId = paymentId,
            Action = action.Trim(),
            ActorReferenceId = actorReferenceId,
            Snapshot = snapshot,
            OccurredAt = DateTime.UtcNow,
        };
    }
}
