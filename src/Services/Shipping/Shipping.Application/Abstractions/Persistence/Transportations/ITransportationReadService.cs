namespace NovaCore.Shipping.Application.Abstractions.Persistence.Transportations;

public interface ITransportationReadService
{
    Task<Transportation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Transportation?> GetByTransportationNumberAsync(string transportationNumber, CancellationToken ct = default);

    /// <summary>Every attempt made against one Shipment, oldest first - the retry history that justifies Shipment/Transportation being separate aggregates.</summary>
    Task<IReadOnlyList<Transportation>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default);
}
