namespace NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProviders;

public interface IShippingProviderWriteService
{

    Task<ShippingProvider> CreateAsync(
        string code,
        string name,
        ProviderType providerType,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
