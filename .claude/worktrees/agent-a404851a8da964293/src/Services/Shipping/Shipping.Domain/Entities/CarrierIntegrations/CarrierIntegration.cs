namespace NovaCore.Shipping.Domain.Entities.CarrierIntegrations;

/// <summary>
/// Connection settings for talking to an external ShippingProvider's API (GHN, VNPost, DHL, ...).
/// Foundation only: this stores *how* to reach the carrier, nothing calls it yet - see
/// Shipping.Infrastructure's IShippingProviderClient, the extension point a later phase
/// implements per carrier.
///
/// Never stores plaintext secrets: ApiKeyRef/SecretRef/WebhookSecretRef are opaque key
/// references, exactly like Payment's GatewayConfiguration - no secret-storage abstraction
/// (Vault, Data Protection) exists in this solution yet.
/// </summary>
public sealed class CarrierIntegration : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid ShippingProviderId { get; private set; }
    public string IntegrationCode { get; private set; } = string.Empty;
    public string BaseUrl { get; private set; } = string.Empty;
    public string? ApiKeyRef { get; private set; }
    public string? SecretRef { get; private set; }
    public string? WebhookSecretRef { get; private set; }
    public IntegrationStatus Status { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }
    public string? LastError { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private CarrierIntegration() { }

    public static CarrierIntegration Create(
        Guid shippingProviderId,
        string integrationCode,
        string baseUrl,
        string? apiKeyRef = null,
        string? secretRef = null,
        string? webhookSecretRef = null)
    {
        if (shippingProviderId == Guid.Empty)
            throw ExceptionFactory.RequiredField("Shipping provider id is required.");

        if (string.IsNullOrWhiteSpace(integrationCode))
            throw ExceptionFactory.RequiredField("Integration code cannot be empty.");

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            throw ExceptionFactory.InvalidFormat("Integration base url must be an absolute URI.");

        return new CarrierIntegration
        {
            Id = Guid.CreateVersion7(),
            ShippingProviderId = shippingProviderId,
            IntegrationCode = integrationCode.Trim().ToUpperInvariant(),
            BaseUrl = baseUrl.Trim(),
            ApiKeyRef = apiKeyRef?.Trim(),
            SecretRef = secretRef?.Trim(),
            WebhookSecretRef = webhookSecretRef?.Trim(),
            Status = apiKeyRef is null ? IntegrationStatus.NotConfigured : IntegrationStatus.Active,
        };
    }

    public void UpdateCredentialRefs(string? apiKeyRef, string? secretRef, string? webhookSecretRef)
    {
        ApiKeyRef = apiKeyRef?.Trim();
        SecretRef = secretRef?.Trim();
        WebhookSecretRef = webhookSecretRef?.Trim();
        Status = apiKeyRef is null ? IntegrationStatus.NotConfigured : IntegrationStatus.Active;
    }

    public void RecordSuccessfulSync()
    {
        LastSyncedAt = DateTime.UtcNow;
        LastError = null;
        Status = IntegrationStatus.Active;
    }

    public void RecordFailure(string error)
    {
        LastError = error?.Trim();
        Status = IntegrationStatus.Failed;
    }

    public void Suspend() => Status = IntegrationStatus.Suspended;
}
