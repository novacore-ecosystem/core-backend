using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationVehicles;
using NovaCore.Shipping.Persistence.Contexts.TransportationVehicles.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationVehicles.Read;

public sealed class TransportationVehicleReadService(ITransportationVehicleRepository repo) : ITransportationVehicleReadService
{
    public async Task<TransportationVehicle?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<TransportationVehicle>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default)
        => await repo.GetByProviderIdAsync(providerId, ct);
}
