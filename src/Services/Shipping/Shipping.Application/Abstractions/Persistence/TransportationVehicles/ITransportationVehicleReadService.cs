namespace NovaCore.Shipping.Application.Abstractions.Persistence.TransportationVehicles;

public interface ITransportationVehicleReadService
{
    Task<TransportationVehicle?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TransportationVehicle>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default);
}
