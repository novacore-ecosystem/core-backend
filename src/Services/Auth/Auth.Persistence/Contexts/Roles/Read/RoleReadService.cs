using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Read;

public sealed class RoleReadService(AuthDbContext dbContext) : IRoleReadService, IPersistenceService
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default)
    {
        return await dbContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid roleId, CancellationToken ct = default)
    {
        var providerKey = roleId.ToString();

        return await dbContext.PermissionGrants
            .AsNoTracking()
            .Where(g => g.ProviderName == PermissionProviderName.Role && g.ProviderKey == providerKey)
            .Select(g => g.PermissionDefinition.Key.Value)
            .ToListAsync(ct);
    }
}
