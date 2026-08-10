using NovaCore.Shipping.Application.Abstractions.Persistence.CarrierIntegrations;
using NovaCore.Shipping.Persistence.Contexts.CarrierIntegrations.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.CarrierIntegrations.Write;

public sealed class CarrierIntegrationWriteService(ICarrierIntegrationRepository repo) : ICarrierIntegrationWriteService
{
    public async Task<CarrierIntegration> CreateAsync(
        Guid shippingProviderId,
        string integrationCode,
        string baseUrl,
        string? apiKeyRef = null,
        string? secretRef = null,
        string? webhookSecretRef = null,
        CancellationToken ct = default)
    {
        var integration = CarrierIntegration.Create(
            shippingProviderId, integrationCode, baseUrl, apiKeyRef, secretRef, webhookSecretRef);

        await repo.AddAsync(integration, ct);

        return integration;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
