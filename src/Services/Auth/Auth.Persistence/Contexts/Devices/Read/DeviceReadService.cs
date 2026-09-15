using NovaCore.Auth.Application.Abstractions.Persistence.Devices;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Contexts.Devices.Repositories;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Devices.Read;

public sealed class DeviceReadService(IDeviceRepository deviceRepo)
    : IDeviceReadService, IPersistenceService
{
    public async Task<Device?> GetByFingerprintAsync(Guid accountId, string fingerprint, CancellationToken ct = default)
    {
        return await deviceRepo.GetByFingerprintAsync(accountId, fingerprint, ct);
    }
}
