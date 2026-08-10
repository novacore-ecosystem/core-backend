namespace NovaCore.Payment.Domain.Entities.Catalogs;

/// <summary>
/// Merchant configuration for a PaymentGateway environment (sandbox/production). Never stores
/// plaintext secrets - ApiKeyRef/SecretRef/WebhookSecretRef are opaque references (key ids) into
/// a future secure secret store. No secret-storage abstraction exists in the solution yet; wiring
/// one is a documented postponed extension point (see docs/services/payment-service.md).
/// </summary>
public sealed class GatewayConfiguration : BaseEntity<Guid>, IAuditable
{
    public Guid GatewayId { get; private set; }
    public GatewayEnvironment Environment { get; private set; }
    public string ApiKeyRef { get; private set; } = string.Empty;
    public string SecretRef { get; private set; } = string.Empty;
    public string? WebhookSecretRef { get; private set; }
    public bool IsActive { get; private set; } = true;

    private GatewayConfiguration() { }

    internal static GatewayConfiguration Create(
        Guid gatewayId,
        GatewayEnvironment environment,
        string apiKeyRef,
        string secretRef,
        string? webhookSecretRef = null)
    {
        if (string.IsNullOrWhiteSpace(apiKeyRef))
            throw ExceptionFactory.RequiredField("Gateway API key reference cannot be empty.");

        if (string.IsNullOrWhiteSpace(secretRef))
            throw ExceptionFactory.RequiredField("Gateway secret reference cannot be empty.");

        return new GatewayConfiguration
        {
            Id = Guid.CreateVersion7(),
            GatewayId = gatewayId,
            Environment = environment,
            ApiKeyRef = apiKeyRef.Trim(),
            SecretRef = secretRef.Trim(),
            WebhookSecretRef = webhookSecretRef?.Trim(),
            IsActive = true,
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
