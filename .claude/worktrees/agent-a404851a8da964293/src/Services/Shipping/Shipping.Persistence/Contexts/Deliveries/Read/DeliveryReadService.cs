using NovaCore.Shipping.Application.Abstractions.Persistence.Deliveries;
using NovaCore.Shipping.Persistence.Contexts.Deliveries.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.Deliveries.Read;

public sealed class DeliveryReadService(IDeliveryRepository repo) : IDeliveryReadService
{
    public async Task<Delivery?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<Delivery?> GetByTransportationIdAsync(Guid transportationId, CancellationToken ct = default)
        => await repo.GetByTransportationIdAsync(transportationId, ct);
}
