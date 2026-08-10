using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Persistence.Contexts.Tenants.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Write;

public sealed class TenantWriteService(
    ITenantRepository repo,
    IUnitOfWork unitOfWork) : ITenantWriteService
{
    public async Task CreateAsync(Tenant tenant, CancellationToken ct = default)
    {
        await repo.AddAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
