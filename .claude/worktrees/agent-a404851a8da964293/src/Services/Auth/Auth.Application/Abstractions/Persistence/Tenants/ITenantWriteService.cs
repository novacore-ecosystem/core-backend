using NovaCore.Auth.Domain.Entities.Tenants;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

public interface ITenantWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(Tenant tenant, CancellationToken ct = default);
}
