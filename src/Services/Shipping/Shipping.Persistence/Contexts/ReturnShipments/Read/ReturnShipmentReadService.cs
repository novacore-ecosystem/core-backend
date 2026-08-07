using NovaCore.Shipping.Application.Abstractions.Persistence.ReturnShipments;
using NovaCore.Shipping.Persistence.Contexts.ReturnShipments.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.ReturnShipments.Read;

public sealed class ReturnShipmentReadService(IReturnShipmentRepository repo) : IReturnShipmentReadService
{
    public async Task<ReturnShipment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<ReturnShipment>> GetByOriginalShipmentIdAsync(Guid originalShipmentId, CancellationToken ct = default)
        => await repo.GetByOriginalShipmentIdAsync(originalShipmentId, ct);
}
