namespace NovaCore.Shipping.Application.Abstractions.Persistence.Deliveries;

public interface IDeliveryReadService
{
    Task<Delivery?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Delivery?> GetByTransportationIdAsync(Guid transportationId, CancellationToken ct = default);
}
