using NovaCore.Auth.Domain.Entities.Scopes;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Scopes;

public interface IScopeReadService
{
    Task<Scope?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
}
