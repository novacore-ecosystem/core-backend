using NovaCore.Auth.Application.Abstractions.Persistence.Devices;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Contexts.Devices.Repositories;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Devices.Write;

/// <summary>Both methods are non-committing - OnAuthenticationSucceededHandler owns the
/// surrounding IUnitOfWork.ExecuteTransactionAsync itself.</summary>
public sealed class DeviceWriteService(IDeviceRepository deviceRepo)
    : IDeviceWriteService, IPersistenceService
{
    public async Task CreateAsync(Device device, CancellationToken ct = default)
    {
        await deviceRepo.AddAsync(device, ct);
    }

    public async Task TouchAsync(Guid deviceId, CancellationToken ct = default)
    {
        await deviceRepo.UpdateAsync(d => d.Id == deviceId, d => d.RecordActivity(), ct);
    }
}
