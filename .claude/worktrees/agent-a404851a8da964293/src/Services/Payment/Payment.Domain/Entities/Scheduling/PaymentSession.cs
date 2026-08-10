namespace NovaCore.Payment.Domain.Entities.Scheduling;

/// <summary>Redirect/checkout session for a PaymentIntent - tracks the hosted-checkout redirect flow independent of Payment/PaymentAttempt.</summary>
public sealed class PaymentSession : AggregateRoot<Guid>, IAuditable
{
    public Guid PaymentIntentId { get; private set; }
    public string? RedirectUrl { get; private set; }
    public string? ReturnUrl { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private PaymentSession() { }

    public static PaymentSession Create(Guid paymentIntentId, string? redirectUrl = null, string? returnUrl = null, DateTime? expiresAt = null)
    {
        return new PaymentSession
        {
            Id = Guid.CreateVersion7(),
            PaymentIntentId = paymentIntentId,
            RedirectUrl = redirectUrl,
            ReturnUrl = returnUrl,
            Status = SessionStatus.Open,
            ExpiresAt = expiresAt,
        };
    }
}
