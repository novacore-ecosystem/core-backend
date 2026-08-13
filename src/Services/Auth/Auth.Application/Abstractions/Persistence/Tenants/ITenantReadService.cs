using NovaCore.Auth.Domain.Entities.Tenants;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

public interface ITenantReadService
{
    /// <summary>Includes Locales - callers needing the full editing/bootstrap payload (detail,
    /// bootstrap) get it in one round trip; lighter callers (existence checks) use GetByCodeAsync/
    /// ExistsByCodeAsync instead.</summary>
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Tenant?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Lean projection (no Locales include) for the version-cache read-through and the
    /// GetTenantVersion gRPC call - both only ever need Version/IsActive, not the full aggregate.</summary>
    Task<(int Version, bool IsActive)?> GetVersionAsync(Guid id, CancellationToken ct = default);

    /// <summary>Database-level search + pagination for the Tenant Management list screen -
    /// matches against Code/Name, case-insensitive. Search happens entirely in the query (no
    /// in-memory filtering); count and page are separate queries against the same filter.</summary>
    Task<(IReadOnlyList<Tenant> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
