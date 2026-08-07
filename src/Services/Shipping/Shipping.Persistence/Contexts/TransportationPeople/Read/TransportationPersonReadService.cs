using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationPeople;
using NovaCore.Shipping.Persistence.Contexts.TransportationPeople.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationPeople.Read;

public sealed class TransportationPersonReadService(ITransportationPersonRepository repo) : ITransportationPersonReadService
{
    public async Task<TransportationPerson?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<TransportationPerson>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default)
        => await repo.GetByProviderIdAsync(providerId, ct);
}
