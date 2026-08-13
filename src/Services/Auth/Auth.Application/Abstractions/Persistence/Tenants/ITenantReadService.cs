using NovaCore.Auth.Domain.Entities.Tenants;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

public interface ITenantReadService
{
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>Database-level search + pagination for the Tenant Management list screen -
    /// matches against Code/Name, case-insensitive. Search happens entirely in the query (no
    /// in-memory filtering); count and page are separate queries against the same filter.</summary>
    Task<(IReadOnlyList<Tenant> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
