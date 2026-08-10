namespace NovaCore.Shipping.Application.Abstractions.Persistence.ReturnShipments;

public interface IReturnShipmentReadService
{
    Task<ReturnShipment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ReturnShipment>> GetByOriginalShipmentIdAsync(Guid originalShipmentId, CancellationToken ct = default);
}
