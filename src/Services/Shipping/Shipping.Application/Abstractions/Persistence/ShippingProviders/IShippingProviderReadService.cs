namespace NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProviders;

public interface IShippingProviderReadService
{
    Task<ShippingProvider?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<ShippingProvider?> GetByCodeAsync(string code, CancellationToken ct = default);
}
