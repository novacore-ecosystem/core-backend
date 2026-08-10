namespace NovaCore.Payment.Domain.Entities.Catalogs;

/// <summary>Maps a gateway-specific status code (e.g. Stripe's "requires_capture") to this service's own PaymentStatus. Only PaymentGateway may construct one.</summary>
public sealed class GatewayStatusMapping : BaseEntity<Guid>, IAuditable
{
    public Guid GatewayId { get; private set; }
    public string GatewayStatusCode { get; private set; } = string.Empty;
    public PaymentStatus MappedStatus { get; private set; }
    public string? Description { get; private set; }

    private GatewayStatusMapping() { }

    internal static GatewayStatusMapping Create(Guid gatewayId, string gatewayStatusCode, PaymentStatus mappedStatus, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(gatewayStatusCode))
            throw ExceptionFactory.RequiredField("Gateway status code cannot be empty.");

        return new GatewayStatusMapping
        {
            Id = Guid.CreateVersion7(),
            GatewayId = gatewayId,
            GatewayStatusCode = gatewayStatusCode.Trim(),
            MappedStatus = mappedStatus,
            Description = description,
        };
    }
}
