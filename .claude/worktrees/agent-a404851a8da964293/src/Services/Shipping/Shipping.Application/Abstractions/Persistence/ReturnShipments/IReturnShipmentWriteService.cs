namespace NovaCore.Shipping.Application.Abstractions.Persistence.ReturnShipments;

public interface IReturnShipmentWriteService
{

    Task<ReturnShipment> CreateAsync(
        Guid originalShipmentId,
        string reason,
        Guid? requestedById = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
