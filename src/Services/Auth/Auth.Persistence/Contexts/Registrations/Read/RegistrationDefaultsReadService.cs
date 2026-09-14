using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Registrations.Read;

public sealed class RegistrationDefaultsReadService(AuthDbContext dbContext)
    : IRegistrationDefaultsReadService, IPersistenceService
{
    public async Task<IReadOnlyCollection<Guid>> GetDefaultRoleIdsAsync(
        Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        return await dbContext.RegistrationDefaultRoles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .Select(x => x.RoleId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<string>> GetDefaultPermissionKeysAsync(
        Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        return await dbContext.RegistrationDefaultPermissions
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .Select(x => x.PermissionDefinition.Key.Value)
            .ToListAsync(ct);
    }
}
