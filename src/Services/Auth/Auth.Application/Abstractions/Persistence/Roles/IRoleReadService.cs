using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Roles;

public interface IRoleReadService
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The stable-identifier lookup used to resolve a well-known Role (e.g. the default
    /// "User" role assigned at registration) without hardcoding its Guid.</summary>
    Task<Role?> GetByCodeAsync(RoleCode code, CancellationToken ct = default);

    Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default);

    /// <summary>The Role's granted permission keys, resolved from the centralized PermissionGrant
    /// table (ProviderName = Role, ProviderKey = roleId) - Role no longer owns a permission
    /// collection itself, see Role's class doc comment.</summary>
    Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid roleId, CancellationToken ct = default);
}
