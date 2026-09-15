using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Devices.Repositories;

public interface IDeviceRepository : IRepository<Device>
{
    /// <summary>Tracked (not AsNoTracking) - the one caller (DeviceWriteService.TouchAsync's
    /// sibling read, and the check-then-create in OnAuthenticationSucceededHandler) needs the
    /// entity ready to mutate without a second round trip.</summary>
    Task<Device?> GetByFingerprintAsync(Guid accountId, string fingerprint, CancellationToken ct = default);
}
