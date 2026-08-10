using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationVehicles;
using NovaCore.Shipping.Persistence.Contexts.TransportationVehicles.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationVehicles.Write;

public sealed class TransportationVehicleWriteService(ITransportationVehicleRepository repo) : ITransportationVehicleWriteService
{
    public async Task<TransportationVehicle> CreateAsync(
        Guid providerId,
        string plateNumber,
        decimal capacityKg,
        string? model = null,
        decimal? capacityM3 = null,
        CancellationToken ct = default)
    {
        var vehicle = TransportationVehicle.Create(providerId, plateNumber, capacityKg, model, capacityM3);

        await repo.AddAsync(vehicle, ct);

        return vehicle;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
