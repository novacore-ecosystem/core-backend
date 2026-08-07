using NovaCore.Shipping.Application.Abstractions.Persistence.ReturnShipments;
using NovaCore.Shipping.Persistence.Contexts.ReturnShipments.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.ReturnShipments.Write;

public sealed class ReturnShipmentWriteService(IReturnShipmentRepository repo) : IReturnShipmentWriteService
{
    public async Task<ReturnShipment> CreateAsync(
        Guid originalShipmentId,
        string reason,
        Guid? requestedById = null,
        CancellationToken ct = default)
    {
        var returnShipment = ReturnShipment.Create(originalShipmentId, reason, requestedById);

        await repo.AddAsync(returnShipment, ct);

        return returnShipment;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
