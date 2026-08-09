using NovaCore.Auth.Domain.Entities.TenantClients;

namespace NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;

public interface ITenantClientWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(TenantClient tenantClient, CancellationToken ct = default);
}
