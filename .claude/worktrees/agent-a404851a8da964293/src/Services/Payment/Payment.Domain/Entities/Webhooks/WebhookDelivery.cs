namespace NovaCore.Payment.Domain.Entities.Webhooks;

/// <summary>Outgoing webhook delivery tracking - this service notifying a consumer module's webhook endpoint about a payment-state change.</summary>
public sealed class WebhookDelivery : AggregateRoot<Guid>, IAuditable
{
    public string TargetUrl { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public WebhookDeliveryStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }

    private WebhookDelivery() { }

    public static WebhookDelivery Create(string targetUrl, string payload)
    {
        if (string.IsNullOrWhiteSpace(targetUrl))
            throw ExceptionFactory.RequiredField("Webhook delivery target URL cannot be empty.");

        if (string.IsNullOrWhiteSpace(payload))
            throw ExceptionFactory.RequiredField("Webhook delivery payload cannot be empty.");

        return new WebhookDelivery
        {
            Id = Guid.CreateVersion7(),
            TargetUrl = targetUrl.Trim(),
            Payload = payload,
            Status = WebhookDeliveryStatus.Pending,
            AttemptCount = 0,
        };
    }
}
