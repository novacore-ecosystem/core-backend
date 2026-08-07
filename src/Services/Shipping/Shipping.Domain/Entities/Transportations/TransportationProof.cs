namespace NovaCore.Shipping.Domain.Entities.Transportations;

/// <summary>
/// Proof-of-handover for one Transportation (receiver name plus signature/photo references).
/// Strict 1:1 extension, so its primary key *is* TransportationId. Deliberately distinct from the
/// Delivery aggregate: proof exists for *every* kind of transportation (including a warehouse
/// transfer handover), whereas Delivery models the customer-facing delivery outcome only.
/// </summary>
public sealed class TransportationProof : BaseEntity, IAuditable
{
    /// <summary>The primary key - shared with Transportation, not a surrogate.</summary>
    public Guid TransportationId { get; private set; }
    public string ReceivedByName { get; private set; } = string.Empty;

    /// <summary>Opaque storage reference, not a blob - no file-storage abstraction exists in this solution yet.</summary>
    public string? SignatureUrl { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string? Note { get; private set; }
    public DateTime CapturedAt { get; private set; }

    private TransportationProof() { }

    internal static TransportationProof Create(
        Guid transportationId,
        string receivedByName,
        string? signatureUrl,
        string? photoUrl,
        string? note = null)
    {
        if (string.IsNullOrWhiteSpace(receivedByName))
            throw ExceptionFactory.RequiredField("Receiver name is required on a delivery proof.");

        return new TransportationProof
        {
            TransportationId = transportationId,
            ReceivedByName = receivedByName.Trim(),
            SignatureUrl = signatureUrl?.Trim(),
            PhotoUrl = photoUrl?.Trim(),
            Note = note?.Trim(),
            CapturedAt = DateTime.UtcNow,
        };
    }
}
