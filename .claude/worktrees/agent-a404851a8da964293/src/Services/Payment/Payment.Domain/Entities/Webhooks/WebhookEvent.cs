namespace NovaCore.Payment.Domain.Entities.Webhooks;

/// <summary>Incoming webhook storage from a gateway - raw payload capture before any processing/verification logic runs.</summary>
public sealed class WebhookEvent : AggregateRoot<Guid>, IAuditable
{
    public Guid GatewayId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string? Signature { get; private set; }
    public WebhookStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private WebhookEvent() { }

    public static WebhookEvent Create(Guid gatewayId, string eventType, string payload, string? signature = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw ExceptionFactory.RequiredField("Webhook event type cannot be empty.");

        if (string.IsNullOrWhiteSpace(payload))
            throw ExceptionFactory.RequiredField("Webhook payload cannot be empty.");

        return new WebhookEvent
        {
            Id = Guid.CreateVersion7(),
            GatewayId = gatewayId,
            EventType = eventType.Trim(),
            Payload = payload,
            Signature = signature,
            Status = WebhookStatus.Received,
            RetryCount = 0,
        };
    }
}
