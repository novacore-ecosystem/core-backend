namespace NovaCore.Payment.Domain.Entities.Payments;

/// <summary>
/// Stripe-style Payment Intent - the entry point of the payment lifecycle
/// (PaymentIntent -&gt; Payment -&gt; PaymentAttempt). Tracks what the caller wants to happen before
/// any gateway-facing Payment/PaymentAttempt exists. Never references a business module type
/// directly - only ReferenceType/ReferenceId.
/// </summary>
public sealed class PaymentIntent : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public ReferenceType ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public Money RequestedAmount { get; private set; } = default!;
    public PaymentIntentStatus Status { get; private set; }
    public string ClientSecret { get; private set; } = string.Empty;
    public DateTime? ExpiresAt { get; private set; }
    public string? Metadata { get; private set; }

    /// <summary>Populated only when explicitly Included - Payment has no ownership over PaymentIntent's lifecycle.</summary>
    public ICollection<Entities.Payments.Payment> Payments { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PaymentIntent() { }

    public static PaymentIntent Create(
        ReferenceType referenceType,
        Guid referenceId,
        Money requestedAmount,
        DateTime? expiresAt = null,
        string? metadata = null)
    {
        return new PaymentIntent
        {
            Id = Guid.CreateVersion7(),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            RequestedAmount = requestedAmount,
            Status = PaymentIntentStatus.Created,
            ClientSecret = GenerateClientSecret(),
            ExpiresAt = expiresAt,
            Metadata = metadata,
        };
    }

    private static string GenerateClientSecret()
        => $"pi_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}";
}
