using NovaCore.Shipping.Application.Abstractions.Persistence.Transportations;
using NovaCore.Shipping.Persistence.Contexts.Transportations.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.Transportations.Read;

public sealed class TransportationReadService(ITransportationRepository repo) : ITransportationReadService
{
    public async Task<Transportation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(
            id,
            query => query
                .Include(t => t.Assignment)
                .Include(t => t.Proof)
                .Include(t => t.Trackings)
                .Include(t => t.Costs),
            ct);

    public async Task<Transportation?> GetByTransportationNumberAsync(string transportationNumber, CancellationToken ct = default)
        => await repo.GetByTransportationNumberAsync(transportationNumber, ct);

    public async Task<IReadOnlyList<Transportation>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default)
        => await repo.GetByShipmentIdAsync(shipmentId, ct);
}
