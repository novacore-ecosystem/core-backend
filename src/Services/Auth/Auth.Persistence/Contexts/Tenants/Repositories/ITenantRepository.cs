using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    /// <summary>Lean (Version, IsActive) projection for the version-cache read-through and the
    /// GetTenantVersion gRPC call - not expressible via the generic Get overloads (no arbitrary
    /// projection support), so it lives here as a custom method.</summary>
    Task<(int Version, bool IsActive)?> GetVersionAsync(Guid id, CancellationToken ct = default);

    /// <summary>Database-level search + pagination for the Tenant Management list screen -
    /// matches against Code/Name, case-insensitive. Not expressible generically (ILike +
    /// Skip/Take + count), so it lives here as a custom method.</summary>
    Task<(IReadOnlyList<Tenant> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
