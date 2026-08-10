namespace NovaCore.Shipping.Application.Abstractions.Persistence.CarrierIntegrations;

public interface ICarrierIntegrationWriteService
{

    Task<CarrierIntegration> CreateAsync(
        Guid shippingProviderId,
        string integrationCode,
        string baseUrl,
        string? apiKeyRef = null,
        string? secretRef = null,
        string? webhookSecretRef = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
