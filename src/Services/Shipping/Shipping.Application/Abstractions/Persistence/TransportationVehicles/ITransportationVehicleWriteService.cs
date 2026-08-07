namespace NovaCore.Shipping.Application.Abstractions.Persistence.TransportationVehicles;

public interface ITransportationVehicleWriteService
{

    Task<TransportationVehicle> CreateAsync(
        Guid providerId,
        string plateNumber,
        decimal capacityKg,
        string? model = null,
        decimal? capacityM3 = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
