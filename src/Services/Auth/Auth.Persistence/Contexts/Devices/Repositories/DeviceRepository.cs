using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Devices.Repositories;

public sealed class DeviceRepository(AuthDbContext dbContext)
    : AuthBaseRepository<Device>(dbContext), IDeviceRepository
{
    public async Task<Device?> GetByFingerprintAsync(Guid accountId, string fingerprint, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.AccountId == accountId && d.Fingerprint == fingerprint, ct);
    }
}
