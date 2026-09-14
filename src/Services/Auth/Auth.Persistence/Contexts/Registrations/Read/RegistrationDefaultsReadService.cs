using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Persistence.Contexts.Registrations.Repositories;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Registrations.Read;

public sealed class RegistrationDefaultsReadService(
    IRegistrationDefaultRoleRepository registrationDefaultRoleRepo,
    IRegistrationDefaultPermissionRepository registrationDefaultPermissionRepo)
    : IRegistrationDefaultsReadService, IPersistenceService
{
    public async Task<IReadOnlyCollection<Guid>> GetDefaultRoleIdsAsync(
        Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        return await registrationDefaultRoleRepo.GetRoleIdsAsync(tenantId, appId, ct);
    }

    public async Task<IReadOnlyCollection<string>> GetDefaultPermissionKeysAsync(
        Guid tenantId, Guid appId, CancellationToken ct = default)
    {
        return await registrationDefaultPermissionRepo.GetPermissionKeysAsync(tenantId, appId, ct);
    }
}
