using NovaCore.Auth.Domain.Entities.Scopes;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Scopes;

public interface IScopeWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(Scope scope, CancellationToken ct = default);
}
