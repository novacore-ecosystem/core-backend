namespace NovaCore.Shipping.Application.Abstractions.Persistence.TransportationPeople;

public interface ITransportationPersonReadService
{
    Task<TransportationPerson?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TransportationPerson>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default);
}
