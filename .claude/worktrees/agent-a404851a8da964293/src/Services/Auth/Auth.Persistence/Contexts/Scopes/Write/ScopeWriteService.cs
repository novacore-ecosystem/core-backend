using NovaCore.Auth.Application.Abstractions.Persistence.Scopes;
using NovaCore.Auth.Domain.Entities.Scopes;
using NovaCore.Auth.Persistence.Contexts.Scopes.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Scopes.Write;

public sealed class ScopeWriteService(
    IScopeRepository repo,
    IUnitOfWork unitOfWork) : IScopeWriteService
{
    public async Task CreateAsync(Scope scope, CancellationToken ct = default)
    {
        await repo.AddAsync(scope, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
